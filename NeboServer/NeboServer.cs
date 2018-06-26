//
//
// Nebo Server enables Nebo web gauges
//
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;
using WebServer;

namespace Nebo
{
    public partial class NeboForm : Form
    {

        // User-defined win32 event
        const int WM_USER_SIMCONNECT = 0x0402;
        private const string EVENT_VALUE = "value";
        private const string EVENT_ID = "id";

        // SimConnect object
        SimConnect simConnect = null;

        enum DataDefinitions
        {
            Dashboard,
            Switches,
            Radio
        }

        enum DataRequests
        {
            Dashboard,
            Switches,
            Radio
        };

        enum NeboEvents
        {
            PANEL,
            STROBE,
            LAND,
            TAXI,
            BCN,
            NAV,
            PITOT,
            PUMP,
            BATTERY,
            ALTERNATOR,
            AVIONICS,
            LANDING_LIGHTS_TOGGLE,

            AP_ALT_HOLD,
            XPNDR_SET,
            AP_HDG_HOLD,
            AP_APR_HOLD,
            AP_NAV1_HOLD,
            AP_BC_HOLD,
            AP_ALT_VAR_INC,
            AP_ALT_VAR_DEC,
            DME1_TOGGLE,
            DME2_TOGGLE,
            MARKER_SOUND_TOGGLE,
            RADIO_ADF_IDENT_TOGGLE,
            RADIO_SELECTED_DME_IDENT_TOGGLE,
            ADF_COMPLETE_SET,
            ADF_FRACT_INC_CARRY,
            ADF_FRACT_DEC_CARRY,
            NAV1_RADIO_FRACT_DEC_CARRY,
            NAV1_RADIO_FRACT_INC_CARRY,
            NAV1_RADIO_WHOLE_INC,
            NAV1_RADIO_SWAP,
            RADIO_VOR1_IDENT_TOGGLE,
            NAV1_STBY_SET,
            NAV2_RADIO_FRACT_DEC_CARRY,
            NAV2_RADIO_FRACT_INC_CARRY,
            NAV2_RADIO_WHOLE_INC,
            NAV2_RADIO_SWAP,
            RADIO_VOR2_IDENT_TOGGLE,
            NAV2_STBY_SET,
            COM1_RADIO_FRACT_INC_CARRY,
            COM1_RADIO_FRACT_DEC_CARRY,
            COM1_RADIO_WHOLE_INC,
            COM1_RADIO_SWAP,
            COM1_TRANSMIT_SELECT,
            COM1_STBY_SET,
            COM2_RADIO_FRACT_INC_CARRY,
            COM2_RADIO_FRACT_DEC_CARRY,
            COM2_RADIO_WHOLE_INC,
            COM2_RADIO_SWAP,
            COM2_TRANSMIT_SELECT,
            COM2_STBY_SET,
            COM_RECEIVE_ALL_TOGGLE,
            MAGNETO_DECR,
            MAGNETO_INCR,
            MAGNETO1_OFF,
            MAGNETO1_RIGHT,
            MAGNETO1_LEFT,
            MAGNETO1_START,
            TOGGLE_MASTER_IGNITION_SWITCH
        };

        enum GROUP_PRIORITIES : uint
        {
            SIMCONNECT_GROUP_PRIORITY_HIGHEST = 1,
            SIMCONNECT_GROUP_PRIORITY_HIGHEST_MASKABLE = 10000000,
            SIMCONNECT_GROUP_PRIORITY_STANDARD = 1900000000,
            SIMCONNECT_GROUP_PRIORITY_DEFAULT = 2000000000,
            SIMCONNECT_GROUP_PRIORITY_LOWEST = 4000000000
        }

        // this is how you declare a data structure so that
        // simconnect knows how to fill it/read it.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct Dashboard
        {
            // this is how you declare a fixed size string
            public float pitch;
            public float roll;
            public float speed;
            public float vspeed;
            public float pressure;
            public float turnrate;
            public int turnball;
            public float altitude;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct Switches
        {
            // this is how you declare a fixed size string
            public uint lights;
            public uint pitot;
            public uint pump;
            public uint avionics;
            public uint battery;
            public uint alternator;
            public uint ignition;
            public uint magnetoLeft;
            public uint magnetoRight;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct Radio
        {
            public uint com1act;
            public uint com1stb;
            public uint com1transmit;
            public uint com2act;
            public uint com2stb;
            public uint com2transmit;
            public uint comRecieveAll;
            public float nav1act;
            public float nav1stb;
            public uint nav1sound;
            public float nav2act;
            public float nav2stb;
            public uint nav2sound;
            public float adf1act;
            public uint adf1sound;
            public uint transponder;
            public int dme1dist;
            public int dme1speed;
            public int dme2dist;
            public int dme2speed;
            public uint dmesound;
            public int dmeselected;
        };

        struct ServerStatus
        {
            public bool connected;
        };

        struct Status
        {
            public string message;
        }

        WebServer.WebServer server;

        interface Polling
        {
            object Data { get; set; }
            bool Queried { get; set; }
            bool Running { get; set; }
            uint IdleUpdates { get; }
            uint MaxIdle { get; }
            DataRequests RequestId { get; }

            void Start(SimConnect simConnect);
            void Stop(SimConnect simConnect);
            void OnData(SIMCONNECT_RECV_SIMOBJECT_DATA data);
            void Configure(SimConnect simconnect);
            void Clear();

        }
        struct Polling<T> : Polling
        {
            public object Data { get; set; }
            public bool Queried { get; set; }
            public bool Running { get; set; }
            public uint IdleUpdates { get; private set; }
            public uint MaxIdle { get; private set; }

            public DataRequests RequestId { get; private set; }
            public DataDefinitions DefintionId { get; private set; }
            private readonly uint pollingInterval;

            public Polling(DataRequests requestiId, DataDefinitions definitionId, uint maxIdle = 1000, uint pollingInterval = 10)
            {
                RequestId = requestiId;
                DefintionId = definitionId;
                this.pollingInterval = pollingInterval;
                Running = false;
                Queried = false;
                Data = null;
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
            public void OnData(SIMCONNECT_RECV_SIMOBJECT_DATA data)
            {
                Data = data.dwData[0];
                IdleUpdates++;
                Queried = false;
            }
            public void Configure(SimConnect simconnect)
            {
                simconnect.RegisterDataDefineStruct<T>(DefintionId);
                Start(simconnect);
            }
        };

        class Pollings : Dictionary<DataRequests, Polling>, IEnumerable<Polling>
        {
            public void ForEach(Action<Polling> action)
            {
                foreach (var p in Values)
                {
                    action(p);
                }
            }

            public void Add(Polling polling)
            {
                Add(polling.RequestId, polling);

            }

            IEnumerator<Polling> IEnumerable<Polling>.GetEnumerator()
            {
                return Values.GetEnumerator();
            }
        }

        Pollings polling = new Pollings()
        {
            { new Polling<Dashboard>(DataRequests.Dashboard, DataDefinitions.Dashboard) },
            { new Polling<Switches>(DataRequests.Switches, DataDefinitions.Switches) },
            { new Polling<Radio>(DataRequests.Radio, DataDefinitions.Radio) }
        };


        public bool HasServer
        {
            get { return server != null; }
        }

        public NeboForm(int port, string dir)
        {
            InitializeComponent();

            labelPort.Text = port.ToString();
            serverDirectory.Text = dir;

            this.server = new WebServer.WebServer($"http://+:{port}/");
            server.AddHandler("/_status", (ctx, _) => ctx.OutputJson(new ServerStatus { connected = simConnect != null }));
            server.AddHandler("*", (ctx, _) => ctx.BrowseDirectory(dir));
            server.AddHandler("/_switches", (ctx, data) => OutputResult(ctx, polling[DataRequests.Switches]) );
            server.AddHandler("/_dashboard", (ctx, data) => OutputResult(ctx, polling[DataRequests.Dashboard]));
            server.AddHandler("/_radios", (ctx, data) => OutputResult(ctx, polling[DataRequests.Radio]));
            server.AddHandler("/_event/{id}/{value}", (ctx, data) => TriggerEventHandler(ctx, data));
            
            try
            { 
                server.Start(async: true);
            }
            catch (HttpListenerException e)
            {
                if (e.Message != "Access is denied") { 
                    throw e;
                }
                server = null;

                return;
            }

            connectToSim();
        }

        private void TriggerEventHandler(HttpListenerContext ctx, Dictionary<string, string> data)
        {
            try
            { 
                var value = UInt32.Parse(data[EVENT_VALUE] ?? "1");
                simConnect.TransmitClientEvent((uint)SIMCONNECT_SIMOBJECT_TYPE.USER, (NeboEvents)Enum.Parse(typeof(NeboEvents), data[EVENT_ID]), value, GROUP_PRIORITIES.SIMCONNECT_GROUP_PRIORITY_HIGHEST, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                ctx.OutputJson(new Status { message = e.Message });
            }
        }

        private void OutputResult(HttpListenerContext ctx, Polling polling)
        {
            if (!polling.Running)
            {
                polling.Start(simConnect);
                // Wait to connect
                System.Threading.Thread.Sleep(200);
            }
            if (polling.Running && polling.Data != null)
            {
                polling.Queried = true;
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                ctx.OutputJson(polling.Data);
            }
            else
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.NoContent;
            }
        }

        // simconnect processing on the main thread.
        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == WM_USER_SIMCONNECT)
            {
                simConnect?.ReceiveMessage();
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }

        private void CloseConnection()
        {
            simConnect?.Dispose();
            simConnect = null;
            polling.ForEach(p => p.Clear());

            DisplayText("Connection closed");
        }

        // Set up all the SimConnect related data definitions and event handlers
        private void InitSimConnect()
        {
            try
            {
                // listen to connect and quit msgs
                simConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(simconnect_OnRecvOpen);
                simConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(simconnect_OnRecvQuit);
                simConnect.OnRecvSystemState += new SimConnect.RecvSystemStateEventHandler(simconnect_OnRecvSystem);

                // listen to exceptions
                simConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(simconnect_OnRecvException);

                // define a data structure
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "PLANE PITCH DEGREES", "Radians", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "PLANE BANK DEGREES", "Radians", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "VERTICAL SPEED", "Feet per second", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "BAROMETER PRESSURE", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "TURN INDICATOR RATE", "Radians per second", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "TURN COORDINATOR BALL", "Position 128", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "INDICATED ALTITUDE", "Feet", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                simConnect.AddToDataDefinition(DataDefinitions.Switches, "LIGHT STATES", "mask", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Switches, "PITOT HEAT", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Switches, "GENERAL ENG FUEL PUMP SWITCH:1", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Switches, "AVIONICS MASTER SWITCH", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Switches, "ELECTRICAL MASTER BATTERY", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Switches, "GENERAL ENG MASTER ALTERNATOR:1", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Switches, "MASTER IGNITION SWITCH", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Switches, "RECIP ENG LEFT MAGNETO:1", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Switches, "RECIP ENG RIGHT MAGNETO:1", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                simConnect.AddToDataDefinition(DataDefinitions.Radio, "COM ACTIVE FREQUENCY:1", "frequency BCD16", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "COM STANDBY FREQUENCY:1", "frequency BCD16", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "COM TRANSMIT:1", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "COM ACTIVE FREQUENCY:2", "frequency BCD16", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "COM STANDBY FREQUENCY:2", "frequency BCD16", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "COM TRANSMIT:2", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "COM RECIEVE ALL", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "NAV ACTIVE FREQUENCY:1", "MHz", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "NAV STANDBY FREQUENCY:1", "MHz", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "NAV SOUND:1", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "NAV ACTIVE FREQUENCY:2", "MHz", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "NAV STANDBY FREQUENCY:2", "MHz", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "NAV SOUND:2", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "ADF ACTIVE FREQUENCY:1", "MHz", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "ADF SOUND:1", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "TRANSPONDER CODE:1", "BCO16", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "NAV DME:1", "Nautical miles", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "NAV DMESPEED:1", "Knots", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "NAV DME:2", "Nautical miles", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "NAV DMESPEED:2", "Knots", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "DME SOUND", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Radio, "SELECTED DME", "Number", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                simConnect.MapClientEventToSimEvent(NeboEvents.PANEL, "PANEL_LIGHTS_TOGGLE");
                simConnect.MapClientEventToSimEvent(NeboEvents.STROBE, "STROBES_TOGGLE");
                simConnect.MapClientEventToSimEvent(NeboEvents.LAND, "LANDING_LIGHTS_TOGGLE");
                simConnect.MapClientEventToSimEvent(NeboEvents.TAXI, "TOGGLE_TAXI_LIGHTS");
                simConnect.MapClientEventToSimEvent(NeboEvents.BCN, "TOGGLE_BEACON_LIGHTS");
                simConnect.MapClientEventToSimEvent(NeboEvents.NAV, "TOGGLE_NAV_LIGHTS");
                simConnect.MapClientEventToSimEvent(NeboEvents.PITOT, "PITOT_HEAT_TOGGLE");
                simConnect.MapClientEventToSimEvent(NeboEvents.PUMP, "TOGGLE_ELECT_FUEL_PUMP");
                simConnect.MapClientEventToSimEvent(NeboEvents.BATTERY, "TOGGLE_MASTER_BATTERY");
                simConnect.MapClientEventToSimEvent(NeboEvents.ALTERNATOR, "TOGGLE_MASTER_ALTERNATOR");
                simConnect.MapClientEventToSimEvent(NeboEvents.AVIONICS, "TOGGLE_AVIONICS_MASTER");

                simConnect.MapClientEventToSimEvent(NeboEvents.XPNDR_SET, "XPNDR_SET");
                simConnect.MapClientEventToSimEvent(NeboEvents.AP_ALT_HOLD, "AP_ALT_HOLD");
                simConnect.MapClientEventToSimEvent(NeboEvents.AP_HDG_HOLD, "AP_HDG_HOLD");
                simConnect.MapClientEventToSimEvent(NeboEvents.AP_APR_HOLD, "AP_APR_HOLD");
                simConnect.MapClientEventToSimEvent(NeboEvents.AP_NAV1_HOLD, "AP_NAV1_HOLD");
                simConnect.MapClientEventToSimEvent(NeboEvents.AP_BC_HOLD, "AP_BC_HOLD");
                simConnect.MapClientEventToSimEvent(NeboEvents.AP_ALT_VAR_INC, "AP_ALT_VAR_INC");
                simConnect.MapClientEventToSimEvent(NeboEvents.AP_ALT_VAR_DEC, "AP_ALT_VAR_DEC");
                simConnect.MapClientEventToSimEvent(NeboEvents.DME1_TOGGLE, "DME1_TOGGLE");
                simConnect.MapClientEventToSimEvent(NeboEvents.DME2_TOGGLE, "DME2_TOGGLE");
                simConnect.MapClientEventToSimEvent(NeboEvents.MARKER_SOUND_TOGGLE, "MARKER_SOUND_TOGGLE");
                simConnect.MapClientEventToSimEvent(NeboEvents.RADIO_ADF_IDENT_TOGGLE, "RADIO_ADF_IDENT_TOGGLE");
                simConnect.MapClientEventToSimEvent(NeboEvents.RADIO_SELECTED_DME_IDENT_TOGGLE, "RADIO_SELECTED_DME_IDENT_TOGGLE");
                simConnect.MapClientEventToSimEvent(NeboEvents.ADF_COMPLETE_SET, "ADF_COMPLETE_SET");
                simConnect.MapClientEventToSimEvent(NeboEvents.ADF_FRACT_INC_CARRY, "ADF_FRACT_INC_CARRY");
                simConnect.MapClientEventToSimEvent(NeboEvents.ADF_FRACT_DEC_CARRY, "ADF_FRACT_DEC_CARRY");

                simConnect.MapClientEventToSimEvent(NeboEvents.NAV1_RADIO_FRACT_DEC_CARRY, "NAV1_RADIO_FRACT_DEC_CARRY");
                simConnect.MapClientEventToSimEvent(NeboEvents.NAV1_RADIO_FRACT_INC_CARRY, "NAV1_RADIO_FRACT_INC_CARRY");
                simConnect.MapClientEventToSimEvent(NeboEvents.NAV1_RADIO_WHOLE_INC, "NAV1_RADIO_WHOLE_INC");
                simConnect.MapClientEventToSimEvent(NeboEvents.NAV1_RADIO_SWAP, "NAV1_RADIO_SWAP");
                simConnect.MapClientEventToSimEvent(NeboEvents.RADIO_VOR1_IDENT_TOGGLE, "RADIO_VOR1_IDENT_TOGGLE");
                simConnect.MapClientEventToSimEvent(NeboEvents.NAV1_STBY_SET, "NAV1_STBY_SET");
                simConnect.MapClientEventToSimEvent(NeboEvents.NAV2_RADIO_FRACT_DEC_CARRY, "NAV2_RADIO_FRACT_DEC_CARRY");
                simConnect.MapClientEventToSimEvent(NeboEvents.NAV2_RADIO_FRACT_INC_CARRY, "NAV2_RADIO_FRACT_INC_CARRY");
                simConnect.MapClientEventToSimEvent(NeboEvents.NAV2_RADIO_WHOLE_INC, "NAV2_RADIO_WHOLE_INC");
                simConnect.MapClientEventToSimEvent(NeboEvents.NAV2_RADIO_SWAP, "NAV2_RADIO_SWAP");
                simConnect.MapClientEventToSimEvent(NeboEvents.RADIO_VOR2_IDENT_TOGGLE, "RADIO_VOR2_IDENT_TOGGLE");
                simConnect.MapClientEventToSimEvent(NeboEvents.NAV2_STBY_SET, "NAV2_STBY_SET");
                simConnect.MapClientEventToSimEvent(NeboEvents.COM1_RADIO_FRACT_INC_CARRY, "COM_RADIO_FRACT_INC_CARRY");
                simConnect.MapClientEventToSimEvent(NeboEvents.COM1_RADIO_FRACT_DEC_CARRY, "COM_RADIO_FRACT_DEC_CARRY");
                simConnect.MapClientEventToSimEvent(NeboEvents.COM1_RADIO_WHOLE_INC, "COM_RADIO_WHOLE_INC");
                simConnect.MapClientEventToSimEvent(NeboEvents.COM1_RADIO_SWAP, "COM_STBY_RADIO_SWAP");
                simConnect.MapClientEventToSimEvent(NeboEvents.COM1_STBY_SET, "COM_STBY_RADIO_SET");
                simConnect.MapClientEventToSimEvent(NeboEvents.COM1_TRANSMIT_SELECT, "COM1_TRANSMIT_SELECT");
                simConnect.MapClientEventToSimEvent(NeboEvents.COM2_RADIO_FRACT_INC_CARRY, "COM2_RADIO_FRACT_INC_CARRY");
                simConnect.MapClientEventToSimEvent(NeboEvents.COM2_RADIO_FRACT_DEC_CARRY, "COM2_RADIO_FRACT_DEC_CARRY");
                simConnect.MapClientEventToSimEvent(NeboEvents.COM2_RADIO_WHOLE_INC, "COM2_RADIO_WHOLE_INC");
                simConnect.MapClientEventToSimEvent(NeboEvents.COM2_RADIO_SWAP, "COM2_RADIO_SWAP");
                simConnect.MapClientEventToSimEvent(NeboEvents.COM2_TRANSMIT_SELECT, "COM2_TRANSMIT_SELECT");
                simConnect.MapClientEventToSimEvent(NeboEvents.COM2_STBY_SET, "COM2_STBY_RADIO_SET");
                simConnect.MapClientEventToSimEvent(NeboEvents.COM_RECEIVE_ALL_TOGGLE, "COM_RECEIVE_ALL_TOGGLE");

                simConnect.MapClientEventToSimEvent(NeboEvents.MAGNETO_DECR, "MAGNETO_DECR");
                simConnect.MapClientEventToSimEvent(NeboEvents.MAGNETO_INCR, "MAGNETO_INCR");
                simConnect.MapClientEventToSimEvent(NeboEvents.MAGNETO1_OFF, "MAGNETO1_OFF");
                simConnect.MapClientEventToSimEvent(NeboEvents.MAGNETO1_RIGHT, "MAGNETO1_RIGHT");
                simConnect.MapClientEventToSimEvent(NeboEvents.MAGNETO1_LEFT, "MAGNETO1_LEFT");
                simConnect.MapClientEventToSimEvent(NeboEvents.MAGNETO1_START, "MAGNETO1_START");
                simConnect.MapClientEventToSimEvent(NeboEvents.TOGGLE_MASTER_IGNITION_SWITCH, "TOGGLE_MASTER_IGNITION_SWITCH");

                // catch a simobject data request
                simConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(simconnect_OnRecvSimobjectDataByType);
                simConnect.OnRecvSimobjectData += new SimConnect.RecvSimobjectDataEventHandler(simconncect_OnRecvSimobjectData);

                // Configure all polling
                polling.ForEach(p => p.Configure(simConnect));

            }
            catch (COMException ex)
            {
                DisplayText($"Exception occured while connecting to simulator {ex.Message}", true);
            }
        }

        void simconnect_OnRecvSystem(SimConnect sender, SIMCONNECT_RECV_SYSTEM_STATE data)
        {
        }

        void simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            DisplayText("Connected to flight simulator", true);
        }

        // The case where the user closes FSX
        void simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            DisplayText("Flight simulator has exited", true);
            CloseConnection();
        }

        void simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            DisplayText("Exception received from flight simulator " + Enum.GetName(typeof(SIMCONNECT_EXCEPTION), (SIMCONNECT_EXCEPTION)data.dwException), true);
        }

        // The case where the user closes the client
        private void NeboForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseConnection();
            server.Stop();
            notifyIconServer.Icon = null;
            notifyIconServer.Dispose();
        }

        void simconncect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            var current = polling[(DataRequests)data.dwRequestID];
            if (current != null) { 
                current.OnData(data);
                if (current.IdleUpdates > current.MaxIdle)
                {
                    current.Stop(sender);
                    DisplayText($"Stopped polling {current.RequestId}");
                }
            }
        }

        void simconnect_OnRecvSimobjectDataByType(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            polling[(DataRequests)data.dwRequestID]?.OnData(data);
        }

        private void connectToSim()
        {
            if (simConnect == null)
            {
                try
                {
                    simConnect = new SimConnect(Application.ProductName, this.Handle, WM_USER_SIMCONNECT, null, 0);

                    InitSimConnect();
                }
                catch (COMException ex)
                {
                    DisplayText($"Unable to connect to flight simulator `{ex.Message}`.", true);
                }
            }
        }

        void DisplayText(string s, bool important = false)
        {
            lastTimestamp.Text = DateTime.Now.ToString();
            if (important && lastStatus.Text != s) { 
                notifyIconServer.ShowBalloonTip(10, Application.ProductName, s, ToolTipIcon.Info);
            }
            lastStatus.Text = s;
        }

        private void timer_click(object sender, EventArgs e)
        {
            if (simConnect == null)
            {
                connectToSim();
            }
        }

        private void notifyIconServer_Click(object sender, EventArgs e)
        {

            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void NeboForm_Load(object sender, EventArgs e)
        {
        }
    }

}
