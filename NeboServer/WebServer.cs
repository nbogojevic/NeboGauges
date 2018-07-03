using System;
using System.Collections.Generic;
using Microsoft.FlightSimulator.SimConnect;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Labs.EmbedIO.Constants;
using Unosquare.Net;
using Unosquare.Swan.Formatters;

namespace Nebo
{
    internal class NeboServer : IDisposable
    {
        private readonly WebServer server;

        internal readonly List<string> ServerLinks = new List<string>();

        internal NeboServer(int port, string dir)
        {
            server = new WebServer($"http://+:{port}/", RoutingStrategy.Regex);
            server.RegisterModule(new WebApiModule());
            server.RegisterModule(new StaticFilesModule(dir));
            server.RegisterModule(new WebSocketsModule());
            server.Module<StaticFilesModule>().UseRamCache = true;
            server.Module<StaticFilesModule>().DefaultExtension = ".html";
            server.Module<WebApiModule>().RegisterController<PollingController>();
            server.Module<WebSocketsModule>().RegisterWebSocketsServer<GaugesWebSocketServer>("/_gauges");
            server.RunAsync();

            // Find host by name
            System.Net.IPHostEntry iphostentry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

            // Enumerate IP addresses
            foreach (System.Net.IPAddress ipaddress in iphostentry.AddressList)
            {
                if (ipaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    ServerLinks.Add($"http://{ipaddress}:{port}/");
                }
            }
        }

        public void Dispose()
        {
            server.Dispose();
        }
    }

    internal class PollingController : WebApiController
    {
        private bool OutputResult(HttpListenerContext ctx, IPolling polling)
        {
            if (!polling.Running)
            {
                polling.Start();
                // Wait to connect
                System.Threading.Thread.Sleep(200);
            }
            if (polling.Running && polling.Data != null)
            {
                polling.Queried();
                ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return ctx.JsonResponse(polling.Data);
            }
            else
            {
                ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.NoContent;
                return true;
            }
        }

        [WebApiHandler(HttpVerbs.Get, "/_status")]
#pragma warning disable RCS1163 // Unused parameter.
        public bool Status(WebServer server, HttpListenerContext context)
        {
            return context.JsonResponse(new { connected = NeboContext.Instance.SimConnect != null });
        }

        [WebApiHandler(HttpVerbs.Get, "/_switches")]
        public bool Switches(WebServer server, HttpListenerContext context)
        {
            return OutputResult(context, NeboContext.Instance.Polling[DataRequests.Switches]);
        }

        [WebApiHandler(HttpVerbs.Get, "/_radios")]
        public bool Radios(WebServer server, HttpListenerContext context)
        {
            return OutputResult(context, NeboContext.Instance.Polling[DataRequests.Radio]);
        }

        [WebApiHandler(HttpVerbs.Get, "/_dashboard")]
        public bool Dashboard(WebServer server, HttpListenerContext context)
        {
            return OutputResult(context, NeboContext.Instance.Polling[DataRequests.Dashboard]);
        }

        [WebApiHandler(HttpVerbs.Post, "/_event/{id}/{value}")]
        public bool Event(WebServer server, HttpListenerContext context, String id, uint value)
#pragma warning restore RCS1163 // Unused parameter.
        {
            try
            {
                NeboContext.Instance.SimConnect.TransmitClientEvent((uint)SIMCONNECT_SIMOBJECT_TYPE.USER, (NeboEvents)Enum.Parse(typeof(NeboEvents), id), value, GROUP_PRIORITIES.SIMCONNECT_GROUP_PRIORITY_HIGHEST, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                context.JsonResponse(new { message = e.Message });
            }
            return true;
        }

        protected bool HandleError(HttpListenerContext context, Exception ex, int statusCode = 500)
        {
            var errorResponse = new
            {
                Title = "Unexpected Error",
                ErrorCode = ex.GetType().Name,
                Description = ex.Message,
            };
            context.Response.StatusCode = statusCode;
            return context.JsonResponse(errorResponse);
        }
    }

    internal class GaugesWebSocketServer : WebSocketsServer
    {
        private readonly Dictionary<DataRequests, HashSet<WebSocketContext>> subscribers = new Dictionary<DataRequests, HashSet<WebSocketContext>>();

        public GaugesWebSocketServer()
            : base(true, 0)
        {
            // placeholder
            foreach (var v in NeboContext.Instance.Polling.Keys)
            {
                subscribers[v] = new HashSet<WebSocketContext>();
                NeboContext.Instance.Polling[v].Configure(this);
            }
        }

        internal bool HasListeners(DataRequests request)
        {
            return subscribers[request].Count > 0;
        }

        /// <summary>
        /// Called when this WebSockets Server receives a full message (EndOfMessage) form a WebSockets client.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="rxBuffer">The rx buffer.</param>
        /// <param name="rxResult">The rx result.</param>
        protected override void OnMessageReceived(WebSocketContext context, byte[] rxBuffer, WebSocketReceiveResult rxResult)
        {
            var result = System.Text.Encoding.UTF8.GetString(rxBuffer);
            DataRequests feed = (DataRequests)Enum.Parse(typeof(DataRequests), result);
            subscribers[feed].Add(context);
            if (!NeboContext.Instance.Polling[feed].Running)
            {
                NeboContext.Instance.Polling[feed].Start();
            }
        }

        /// <summary>
        /// Gets the name of the server.
        /// </summary>
        /// <value>
        /// The name of the server.
        /// </value>
        public override string ServerName
        {
            get { return "Gauge Server"; }
        }

        /// <summary>
        /// Called when this WebSockets Server accepts a new WebSockets client.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void OnClientConnected(WebSocketContext context)
        {
        }

        /// <summary>
        /// Called when this WebSockets Server receives a message frame regardless if the frame represents the EndOfMessage.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="rxBuffer">The rx buffer.</param>
        /// <param name="rxResult">The rx result.</param>
        protected override void OnFrameReceived(WebSocketContext context, byte[] rxBuffer, WebSocketReceiveResult rxResult)
        {
        }

        /// <summary>
        /// Called when the server has removed a WebSockets connected client for any reason.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void OnClientDisconnected(WebSocketContext context)
        {
            foreach (var v in subscribers.Values)
            {
                v.Remove(context);
            }
        }

        internal bool OnDataRequest(DataRequests feed, object data)
        {
            HashSet<WebSocketContext> existingSubscribers = subscribers[feed];
            String json = Json.Serialize(new { feed = feed.ToString(), data });
            foreach (var ctx in existingSubscribers)
            {
                this.Send(ctx, json);
            }
            return existingSubscribers.Count > 0;
        }
    }
}
