//
//
// Nebo Server enables Nebo web gauges
//
//

using System;
using System.Windows.Forms;
using QRCoder;
using System.Drawing;

namespace Nebo
{
    public partial class NeboForm : Form
    {
        public NeboForm(int port, string dir)
        {
            InitializeComponent();

            serverDirectory.Text = dir;

            NeboContext.Instance.WebServer = new NeboServer(port, dir);
            serverLink.Text = NeboContext.Instance.WebServer.ServerLinks[0];

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(serverLink.Text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            qrCodeBox.Image = qrCode.GetGraphic(20);

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

        public void Notify(string s, ToolTipIcon icon = ToolTipIcon.None)
        {
            lastTimestamp.Text = DateTime.Now.ToString();
            if (lastStatus.Text != s)
            {
                notifyIconServer.ShowBalloonTip(10, Application.ProductName, s, icon);
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

        private void serverLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(serverLink.Text);
        }

        private void generateQRCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start($"http://chart.apis.google.com/chart?cht=qr&chs=300x300&chl={System.Net.WebUtility.UrlEncode(serverLink.Text)}/&chld=H|0");
        }
    }
}
