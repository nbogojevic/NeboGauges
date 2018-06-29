using System;
using System.Collections.Generic;
using Microsoft.FlightSimulator.SimConnect;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Labs.EmbedIO.Constants;
using Unosquare.Net;
using Unosquare.Swan.Formatters;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Nebo
{
    interface IPolling
    {
        object Data { get; set; }
        bool Queried { get; set; }
        bool Running { get; set; }
        uint IdleUpdates { get; }
        uint MaxIdle { get; }
        DataRequests RequestId { get; }
        bool HasListeners { get; }

        void Start(SimConnect simconnect);
        void Stop(SimConnect simconnect);
        void OnData(SimConnect simconnect, SIMCONNECT_RECV_SIMOBJECT_DATA data, ILog log = null);
        void Configure(SimConnect simconnect);
        void Clear();
        void Configure(GaugesWebSocketServer gaugesWebSocketServer);
        void Start();
    }

    struct Polling<T> : IPolling
    {
        public object Data { get; set; }
        public bool Queried { get; set; }
        public bool Running { get; set; }
        public uint IdleUpdates { get; private set; }
        public uint MaxIdle { get; private set; }
        public bool HasListeners { get => socketServer != null; }

        public DataRequests RequestId { get; private set; }
        public DataDefinitions DefintionId { get; private set; }
        private readonly uint pollingInterval;
        private GaugesWebSocketServer socketServer;

        public Polling(DataRequests requestiId, DataDefinitions definitionId, uint pollingInterval = 10, uint maxIdle = 1000)
        {
            RequestId = requestiId;
            DefintionId = definitionId;
            this.pollingInterval = pollingInterval;
            Running = false;
            Queried = false;
            Data = null;
            socketServer = null;
            IdleUpdates = 0;
            MaxIdle = maxIdle;
        }
        public void Start(SimConnect simconnect)
        {
            simconnect.RequestDataOnSimObject(RequestId, DefintionId, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.VISUAL_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, pollingInterval, 0);
            Clear();
            Running = true;
        }
        public void Clear()
        {
            Running = false;
            Queried = false;
            Data = null;
            IdleUpdates = 0;
        }
        public void Stop(SimConnect simconnect)
        {
            simconnect.RequestDataOnSimObject(RequestId, DefintionId, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.NEVER, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, uint.MaxValue, 0);
            Clear();
        }
        public void OnData(SimConnect simconnect, SIMCONNECT_RECV_SIMOBJECT_DATA data, ILog log)
        {
            Data = data.dwData[0];
            IdleUpdates++;
            Queried = false;
            if (socketServer != null)
            {
                socketServer.OnDataRequest(RequestId, Data);
            }
            if (IdleUpdates > MaxIdle && !HasListeners)
            {
                Stop(simconnect);
                log?.Log($"Stopped polling {RequestId}");
            }

        }
        public void Configure(SimConnect simconnect)
        {
            if (simconnect != null)
            {
                simconnect.RegisterDataDefineStruct<T>(DefintionId);
                Start(simconnect);
            }
        }
        public void Configure(GaugesWebSocketServer socketServer)
        {
            this.socketServer = socketServer;
        }

        public void Start()
        {
            if (NeboContext.Instance.SimConnect != null)
            {
                Start(NeboContext.Instance.SimConnect);
            }
        }
    };


    class PollingDictionary : Dictionary<DataRequests, IPolling>, IEnumerable<IPolling>
    {
        internal void ForEach(Action<IPolling> action)
        {
            foreach (var p in Values)
            {
                action(p);
            }
        }

        internal void Add(IPolling polling)
        {
            Add(polling.RequestId, polling);

        }

        IEnumerator<IPolling> IEnumerable<IPolling>.GetEnumerator()
        {
            return Values.GetEnumerator();
        }
    }

    class NeboContext : IDisposable
    {
        internal const int WM_USER_SIMCONNECT = 0x0402;

        internal SimConnect SimConnect { get; set; }
        internal NeboServer WebServer { get; set; }
        internal bool HasServer { get => WebServer != null; }
        internal readonly PollingDictionary Polling = new PollingDictionary()
        {
            { new Polling<Dashboard>(DataRequests.Dashboard, DataDefinitions.Dashboard) },
            { new Polling<Switches>(DataRequests.Switches, DataDefinitions.Switches) },
            { new Polling<Radio>(DataRequests.Radio, DataDefinitions.Radio) }
        };

        internal static NeboContext Instance = new NeboContext();

        internal void CloseConnection()
        {
            SimConnect?.Dispose();
            SimConnect = null;
            Polling.ForEach(p => p.Clear());
        }

        public void Dispose()
        {
            CloseConnection();
            WebServer?.Dispose();
        }

        internal void ConnectToSim(IntPtr handle, ILog log = null)
        {
            if (SimConnect == null)
            {
                try
                {
                    SimConnect = new SimConnect(Application.ProductName, handle, WM_USER_SIMCONNECT, null, 0);
                    SimConnect.Configure(log);
                }
                catch (COMException ex)
                {
                    SimConnect?.Dispose();
                    SimConnect = null;
                    log?.Notify($"Exception occured while connecting to simulator {ex.Message}");
                }
            }
        }

    };
}
