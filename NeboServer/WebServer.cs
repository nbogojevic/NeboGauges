using System;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace WebServer
{
    public delegate void RouteAction(HttpListenerContext ctx, Dictionary<string, string> data);

    public class WebServer : IDisposable
    {
        private HttpListener _httpListener;
        private Router _router;

        public WebServer(string urls)
        {
            _httpListener = new HttpListener();
            urls.Split('|').ToList().ForEach(_httpListener.Prefixes.Add);
            _router = new Router();
        }

        public void AddHandler(string path, RouteAction fn) { _router.Add(path, fn); }

        public void Start(bool async = false)
        {
            Action fnWorker = () =>
            {
                while (_httpListener != null && _httpListener.IsListening)
                {
                    try { 
                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            var ctx = state as HttpListenerContext;
                            try
                            {
                                RouteAction fnRoute;
                                Dictionary<string, string> data;
                                if (ctx.Request.HttpMethod == "OPTIONS")
                                {
                                    ctx.Response.AppendHeader("Access-Control-Allow-Headers", "DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range");
                                    ctx.Response.AppendHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS'");
                                    ctx.Response.AppendHeader("Access-Control-Max-Age", "1728000");
                                    ctx.Response.AppendHeader("Content-Type", "text/plain; charset=utf-8");
                                    ctx.Response.AppendHeader("Content-Length", "0");
                                    ctx.Response.StatusCode = (int)HttpStatusCode.NoContent;
                                    return;
                                }
//                                ctx.Response.AppendHeader("Access-Control-Allow-Headers", "DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range");
//                                ctx.Response.AppendHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS'");
//                                ctx.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                                if (_router.TryGetValue(ctx.Request.Url.LocalPath, out fnRoute, out data)
                                        || _router.TryGetValue("*", out fnRoute, out data))
                                    fnRoute(ctx, data);
                                else
                                    ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            }
                            catch (Exception e)
                            {
                                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                ctx.OutputText($"Exception caught: {e.Message}");
                            }
                            try { ctx.Response.Close(); }
                            catch { }
                        }, _httpListener.GetContext());
                    }
                    catch (HttpListenerException e)
                    {
                        if (_httpListener != null && _httpListener.IsListening) { 
                            Console.WriteLine("Exception in server loop: " + e.Message);
                        }
                        // Exception while doing loop
                    }
                }
            };
            _httpListener.Start();
            if (async)
                ThreadPool.QueueUserWorkItem(_ => fnWorker());
            else
                fnWorker();
        }

        public void Stop() {
            _httpListener.Stop();
        }

        public void Dispose()
        {
            _httpListener?.Close();
            _httpListener = null;
            (_router as IDisposable)?.Dispose();
            _router = null;
        }
    }

    // Each new route is assigned a key from permutations of `KeyBase` ("123456") and is stored in 
    // `_routes` dictionary. Router implementation builds a composite regex from all routes 
    // patterns that looks like
    //    route_pattern1 | route_pattern2 | route_pattern3 | route_pattern4 | ...
    // where `route_patternN` is prefixed with it's key pattern that looks like
    //    ^(?<__c1__>1)(?<__c5__>2)(?<__c3__>3)(?<__c2__>4)(?<__c4__>5)(?<__c6__>6)
    // These key patterns always match `KeyBase` ("123456") but in different named captures, so in the 
    // sample key pattern above when matched against "123456/local/path" the `__c1__` to `__c6__` 
    // named captures will concatenate to "143526" for currently matched route key. The corresponding 
    // entry in `_routes` has `GroupStart` to `GroupEnd` that are used to extract handler data 
    // dictionary from the composite regex anonymous captures.
    internal class Router : IDisposable
    {
        private static readonly string KeyBase = "123456";
        private static readonly Regex RoutePattern = new Regex(@"(/(({(?<data>[^}/:]+)(:(?<type>[^}/]+))?}?)|(?<static>[^/]+))|\*)",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        private class RouteEntry
        {
            public string Pattern { get; set; }
            public int GroupStart { get; set; }
            public int GroupEnd { get; set; }
            public RouteAction Handler { get; set; }
        }

        private Dictionary<string, RouteEntry> _routes = new Dictionary<string, RouteEntry>();
        private IEnumerator<IEnumerable<char>> _permEnum = GetPermutations(KeyBase.ToCharArray(), KeyBase.Length).GetEnumerator();
        private string[] _groupNames = new string[32];
        private Regex _pathParser;

        public void Add(string route, RouteAction handler)
        {
            // for each "{key:type}" check regex pattern in `type` and raise `ArgumentException` on failure
            RoutePattern.Replace(route, m =>
            {
                if (string.IsNullOrEmpty(m.Groups["static"].Value) && !string.IsNullOrEmpty(m.Groups["data"].Value)
                        && !string.IsNullOrEmpty(m.Groups["type"].Value))
                    Regex.Match("", m.Groups["type"].Value);
                return null;
            });
            _permEnum.MoveNext();
            _routes.Add(string.Join(null, _permEnum.Current), new RouteEntry { Pattern = route, Handler = handler });
            _pathParser = null;
        }

        public bool TryGetValue(string localPath, out RouteAction handler, out Dictionary<string, string> data)
        {
            handler = null;
            data = null;
            if (_pathParser == null)
                _pathParser = RebuildParser();
            var match = _pathParser.Match(KeyBase + localPath);
            if (match.Success)
            {
                string routeKey = null;
                for (int idx = 1; idx <= KeyBase.Length; idx++)
                    routeKey += match.Groups[$"__c{idx}__"].Value;
                var entry = _routes[routeKey];
                handler = entry.Handler;
                if (entry.GroupStart < entry.GroupEnd)
                    data = new Dictionary<string, string>();
                for (var groupIdx = entry.GroupStart; groupIdx < entry.GroupEnd; groupIdx++)
                    data[_groupNames[groupIdx]] = match.Groups[groupIdx].Value;
            }
            return match.Success;
        }

        private Regex RebuildParser()
        {
            string[] rev = new string[KeyBase.Length];
            var sb = new StringBuilder();
            int groupIdx = 1;

            foreach (string key in _routes.Keys)
            {
                var entry = _routes[key];
                entry.GroupStart = groupIdx;
                int el = 1;
                foreach (char c in key.ToCharArray())
                    rev[c - '1'] = $"(?<__c{el++}__>{c})";
                sb.AppendLine((sb.Length > 0 ? "|" : null) + "^" + string.Join(null, rev) +
                    RoutePattern.Replace(entry.Pattern, m =>
                    {
                        string str = m.Groups["static"].Value;
                        if (!string.IsNullOrEmpty(str))
                            return "/" + Regex.Escape(str);
                        str = m.Groups["data"].Value;
                        if (!string.IsNullOrEmpty(str))
                        {
                            if (groupIdx >= _groupNames.Length)
                                Array.Resize(ref _groupNames, _groupNames.Length * 2);
                            _groupNames[groupIdx++] = str;
                            str = m.Groups["type"].Value;
                            return $"/({(string.IsNullOrEmpty(str) ? "[^/]*" : str)})";
                        }
                        return Regex.Escape(m.Groups[0].Value);
                    }));
                entry.GroupEnd = groupIdx;
            }
            return new Regex(sb.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }

        public void Dispose()
        {
            _permEnum?.Dispose();
            _permEnum = null;
        }

        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1).SelectMany(t => list
                .Where(o => !t.Contains(o)), (t1, t2) => t1.Concat(new T[] { t2 }));
        }
    }

    internal static class ContextExtensions
    {
        public static void OutputJson(this HttpListenerContext ctx, object o, string contentType = "application/json", Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            OutputBinary(ctx, encoding.GetBytes(PetaJson.Json.Format(o, PetaJson.JsonOptions.DontWriteWhitespace)), $"{contentType}; charset={encoding.WebName}");
        }

        public static void OutputUtf8(this HttpListenerContext ctx, string html, string contentType = "text/html", Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            OutputBinary(ctx, encoding.GetBytes(html), $"{contentType}; charset={encoding.WebName}");
        }

        public static void OutputText(this HttpListenerContext ctx, string text, string contentType = "text/plain", Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.Default;
            OutputBinary(ctx, encoding.GetBytes(text), $"{contentType}; charset={encoding.WebName}");
        }

        public static void OutputBinary(this HttpListenerContext ctx, byte[] content, string contentType = "application/octet-stream")
        {
            ctx.Response.ContentType = contentType;
            ctx.Response.ContentLength64 = content.Length;
            ctx.Response.OutputStream.Write(content, 0, content.Length);
        }

        public static void AddFromMembers(this WebServer server, object callback)
        {
            var methods = callback.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod);
            foreach (MethodInfo method in methods)
            {
                var parms = method.GetParameters();
                if (parms.Length == 1 && parms[0].GetType().Name == "RouteAction")
                    server.AddHandler("/" + method.Name, (ctx, data) => method.Invoke(callback, new object[] { ctx }));
            }
        }
        public static void BrowseDirectory(this HttpListenerContext ctx, string rootFolder)
        {
            if (!string.IsNullOrEmpty(rootFolder) && !rootFolder.EndsWith(@"\"))
                rootFolder += @"\";
            else
                rootFolder = rootFolder ?? string.Empty;
            var requestedPath = ctx.Request.Url.LocalPath.Substring(1).Replace('/', '\\');
            var path = Path.Combine(rootFolder, requestedPath);
            if (File.Exists(path))
            {
                var mimeType = Registry.GetValue($"HKEY_CLASSES_ROOT\\{Path.GetExtension(path)}", "Content Type", null) as string ?? "application/octet-stream";
                if (string.Equals(mimeType.Substring(0, 5), "text/", StringComparison.OrdinalIgnoreCase))
                    ctx.OutputUtf8(File.ReadAllText(path), mimeType);
                else
                    ctx.OutputBinary(File.ReadAllBytes(path), mimeType);
            }
            else if (Directory.Exists(path))
            {
                var html = new StringBuilder($@"<html>
<body>
    <h1>Listing of /{requestedPath}</h1>
    <table style='font-family: courier; padding: 10px;'>
        <th style='min-width: 300px;'>Name</th><th>Last modified</th><th style='min-width: 90px;'>Size</th>");
                var dirInfo = new DirectoryInfo(path);
                var url = dirInfo.Parent?.FullName;
                if (!string.IsNullOrEmpty(url) && url.Length >= rootFolder.Length - 1)
                {
                    if (!url.EndsWith(@"\"))
                        url += @"\";
                    url = "/" + url.Substring(rootFolder.Length).Replace('\\', '/');
                    html.AppendLine($"<tr><td><a href=\"{url}\">Parent Directory</a></td><td>&nbsp;</td><td align=\"right\">&lt;DIR&gt;</td></tr>");
                }
                foreach (var dir in dirInfo.EnumerateDirectories())
                {
                    url = "/" + dir.FullName.Substring(rootFolder.Length).Replace('\\', '/');
                    html.AppendLine($"<tr><td><a href=\"{url}\">{WebUtility.HtmlEncode(dir.Name)}</a></td><td>{dir.LastWriteTime:yyyy-MMM-dd hh:mm:ss}</td><td align=\"right\">&lt;DIR&gt;</td></tr>\n");
                }
                foreach (var file in dirInfo.EnumerateFiles())
                {
                    url = "/" + file.FullName.Substring(rootFolder.Length).Replace('\\', '/');
                    html.AppendLine($"<tr><td><a href=\"{url}\">{WebUtility.HtmlEncode(file.Name)}</a></td><td>{file.LastWriteTime:yyyy-MMM-dd hh:mm:ss}</td><td align=\"right\">{file.Length:#,#}</td></tr>");
                }
                html.AppendLine("</table>\n</body>\n</html>");
                ctx.OutputUtf8(html.ToString());
            }
            else
                ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
    }

    /*
    simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.Radio, DEFINITIONS.Radio, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
    Console.WriteLine("{0} {1}", ctx.Request.HttpMethod, ctx.Request.Url.LocalPath);
    var str = string.Join(", ", data.Keys.Select(el => $"{el}={data[el]}"));
    Console.WriteLine("  -> {0}", str);
    ctx.OutputUtf8(str);
    */
}