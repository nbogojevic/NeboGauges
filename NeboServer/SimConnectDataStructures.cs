using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace Nebo
{
    enum DataDefinitions
    {
        Dashboard,
        Switches,
        Radio
    }

    public enum DataRequests
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
        TOGGLE_MASTER_IGNITION_SWITCH,
        HEADING_BUG_INC,
        HEADING_BUG_DEC,
        KOHLSMAN_INC,
        KOHLSMAN_DEC,
        GYRO_DRIFT_INC,
        GYRO_DRIFT_DEC,
        TRUE_AIRSPEED_CAL_INC,
        TRUE_AIRSPEED_CAL_DEC
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
        public float heading;
        public int cdiNeedle;
        public int gsiNeedle;
        public int offValue;
        public float navRadial;
        public float adfRadial;
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

    static class NeboSimConnectExtension
    {
        internal static void Configure(this SimConnect simConnect, Action reconnect, Action<string> log)
        {
            try
            {
                // listen to connect and quit msgs
                simConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler((sender, data) => log?.Invoke("Connected to flight simulator"));
                simConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler((sender, data) =>
                {
                    log?.Invoke("Flight simulator has exited");
                    NeboContext.Instance.CloseConnection();
                    reconnect?.Invoke();
                });
                simConnect.OnRecvSystemState += new SimConnect.RecvSystemStateEventHandler((sender, data) => { });

                // listen to exceptions
                simConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler((sender, data) =>
                {
                    log?.Invoke($"Exception received from flight simulator { Enum.GetName(typeof(SIMCONNECT_EXCEPTION), (SIMCONNECT_EXCEPTION)data.dwException) }");
                });
                // define a data structure
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "PLANE PITCH DEGREES", "Radians", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "PLANE BANK DEGREES", "Radians", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "VERTICAL SPEED", "Feet per second", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "BAROMETER PRESSURE", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "TURN INDICATOR RATE", "Radians per second", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "TURN COORDINATOR BALL", "Position 128", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "INDICATED ALTITUDE", "Feet", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "HEADING INDICATOR", "Radians", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "NAV CDI:1", "Number", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "NAV GSI:1", "Number", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "NAV TOFROM:1", "Enum", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "NAV RADIAL:2", "Degrees", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DataDefinitions.Dashboard, "ADF RADIAL:1", "Degrees", SIMCONNECT_DATATYPE.FLOAT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);

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

                simConnect.MapClientEventToSimEvent(NeboEvents.HEADING_BUG_INC, "HEADING_BUG_INC");
                simConnect.MapClientEventToSimEvent(NeboEvents.HEADING_BUG_DEC, "HEADING_BUG_DEC");
                simConnect.MapClientEventToSimEvent(NeboEvents.KOHLSMAN_INC, "KOHLSMAN_INC");
                simConnect.MapClientEventToSimEvent(NeboEvents.KOHLSMAN_DEC, "KOHLSMAN_DEC");
                simConnect.MapClientEventToSimEvent(NeboEvents.GYRO_DRIFT_INC, "GYRO_DRIFT_INC");
                simConnect.MapClientEventToSimEvent(NeboEvents.GYRO_DRIFT_DEC, "GYRO_DRIFT_DEC");
                simConnect.MapClientEventToSimEvent(NeboEvents.TRUE_AIRSPEED_CAL_INC, "TRUE_AIRSPEED_CAL_INC");
                simConnect.MapClientEventToSimEvent(NeboEvents.TRUE_AIRSPEED_CAL_DEC, "TRUE_AIRSPEED_CAL_DEC");

                // catch a simobject data request
                simConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler((sender, data) => NeboContext.Instance.Polling[(DataRequests)data.dwRequestID]?.OnData(sender, data, log));
                simConnect.OnRecvSimobjectData += new SimConnect.RecvSimobjectDataEventHandler((sender, data) => NeboContext.Instance.Polling[(DataRequests)data.dwRequestID]?.OnData(sender, data, log));

                // Configure all polling
                NeboContext.Instance.Polling.ForEach(p => p.Configure(simConnect));

            }
            catch (COMException ex)
            {
                log?.Invoke($"Exception occured while connecting to simulator {ex.Message}");
                reconnect?.Invoke();
            }
        }
    }
}
