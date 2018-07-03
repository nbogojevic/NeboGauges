using System;
using System.Collections.Generic;
using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Nebo
{
    internal interface IPolling
    {
        object Data { get; set; }
        bool Running { get; set; }
        uint IdleUpdates { get; }
        uint MaxIdle { get; }
        DataRequests RequestId { get; }
        bool HasListeners { get; }

        void Start(SimConnect simconnect);
        void Stop(SimConnect simconnect);
        void OnData(SimConnect simconnect, SIMCONNECT_RECV_SIMOBJECT_DATA data, Action<string, ToolTipIcon> log);
        void Configure(SimConnect simconnect);
        void Clear();
        void Queried();
        void Configure(GaugesWebSocketServer gaugesWebSocketServer);
        void Start();
    }

    internal struct Polling<T> : IPolling
    {
        public object Data { get; set; }
        public bool Running { get; set; }
        public uint IdleUpdates { get; private set; }
        public uint MaxIdle { get; }
        public bool HasListeners { get => socketServer?.HasListeners(RequestId) == true; }

        public DataRequests RequestId { get; }
        public DataDefinitions DefintionId { get; }
        private readonly uint pollingInterval;
        private GaugesWebSocketServer socketServer;

        public Polling(DataRequests requestiId, DataDefinitions definitionId, uint pollingInterval = 10, uint maxIdle = 1000)
        {
            RequestId = requestiId;
            DefintionId = definitionId;
            this.pollingInterval = pollingInterval;
            Running = false;
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
            Data = null;
            IdleUpdates = 0;
        }

        public void Stop(SimConnect simconnect)
        {
            simconnect.RequestDataOnSimObject(RequestId, DefintionId, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.NEVER, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, uint.MaxValue, 0);
            Clear();
        }

        public void OnData(SimConnect simconnect, SIMCONNECT_RECV_SIMOBJECT_DATA data, Action<string, ToolTipIcon> log)
        {
            Data = data.dwData[0];
            IdleUpdates++;
            if (socketServer?.OnDataRequest(RequestId, Data) == true)
            {
                Queried();
            }
            if (IdleUpdates > MaxIdle && !HasListeners)
            {
                Stop(simconnect);
                log?.Invoke($"Stopped polling {RequestId}", ToolTipIcon.None);
            }
        }

        public void Queried()
        {
            IdleUpdates = 0;
        }

        public void Configure(SimConnect simconnect)
        {
            if (simconnect != null)
            {
                simconnect.RegisterDataDefineStruct<T>(DefintionId);
                Start(simconnect);
            }
        }

        public void Configure(GaugesWebSocketServer gaugesWebSocketServer)
        {
            this.socketServer = gaugesWebSocketServer;
        }

        public void Start()
        {
            if (NeboContext.Instance.SimConnect != null)
            {
                Start(NeboContext.Instance.SimConnect);
            }
        }
    };

    internal class PollingDictionary : Dictionary<DataRequests, IPolling>, IEnumerable<IPolling>
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

    internal class NeboContext : IDisposable
    {
        internal const int WM_USER_SIMCONNECT = 0x0402;

        internal SimConnect SimConnect { get; private set; }

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

        internal bool ConnectToSim(IntPtr handle, Action<string, ToolTipIcon> log, Action reconnect)
        {
            if (SimConnect == null)
            {
                try
                {
                    SimConnect = new SimConnect(Application.ProductName, handle, WM_USER_SIMCONNECT, null, 0);
                    SimConnect.Configure(reconnect, log);
                }
                catch (COMException ex)
                {
                    SimConnect?.Dispose();
                    SimConnect = null;
                    log?.Invoke($"Unable to connect to flight simulator {ex.Message}", ToolTipIcon.Warning);
                }
            }
            return SimConnect != null;
        }
    }
}
