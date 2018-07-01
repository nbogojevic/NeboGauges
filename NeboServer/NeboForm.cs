//
//
// Nebo Server enables Nebo web gauges
//
//

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Labs.EmbedIO.Constants;

namespace Nebo
{
    public partial class NeboForm : Form
    {

        public NeboForm(int port, string dir)
        {
            InitializeComponent();

            labelPort.Text = port.ToString();
            serverDirectory.Text = dir;

            NeboContext.Instance.WebServer = new NeboServer(port, dir);

            NeboContext.Instance.ConnectToSim(Handle, Notify, () => reconnectTimer.Start());
        }

        // simconnect processing on the main thread.
        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == NeboContext.WM_USER_SIMCONNECT)
            {
                NeboContext.Instance.SimConnect?.ReceiveMessage();
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }

        // The case where the user closes the client
        private void NeboForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            NeboContext.Instance.Dispose();
            notifyIconServer.Icon = null;
            notifyIconServer.Dispose();
        }

        public void Notify(string s)
        {
            lastTimestamp.Text = DateTime.Now.ToString();
            if (lastStatus.Text != s)
            {
                notifyIconServer.ShowBalloonTip(10, Application.ProductName, s, ToolTipIcon.Info);
            }
            lastStatus.Text = s;
        }

        private void timer_click(object sender, EventArgs e)
        {
            if (NeboContext.Instance.ConnectToSim(Handle, Notify, () => reconnectTimer.Start()))
            {
                reconnectTimer.Stop();
            }
        }

        private void notifyIconServer_Click(object sender, EventArgs e)
        {
            this.Activate();
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Focus();
        }

        private void NeboForm_Load(object sender, EventArgs e)
        {
        }
    }

}
