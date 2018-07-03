namespace Nebo
{
    partial class NeboForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NeboForm));
            this.labelLatest = new System.Windows.Forms.Label();
            this.reconnectTimer = new System.Windows.Forms.Timer(this.components);
            this.lastTimestamp = new System.Windows.Forms.Label();
            this.lastStatus = new System.Windows.Forms.Label();
            this.notifyIconServer = new System.Windows.Forms.NotifyIcon(this.components);
            this.labelListening = new System.Windows.Forms.Label();
            this.labelDirectory = new System.Windows.Forms.Label();
            this.serverDirectory = new System.Windows.Forms.Label();
            this.serverLink = new System.Windows.Forms.LinkLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.qrCodeBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.qrCodeBox)).BeginInit();
            this.SuspendLayout();
            // 
            // labelLatest
            // 
            this.labelLatest.AutoSize = true;
            this.labelLatest.Location = new System.Drawing.Point(28, 63);
            this.labelLatest.Name = "labelLatest";
            this.labelLatest.Size = new System.Drawing.Size(62, 13);
            this.labelLatest.TabIndex = 4;
            this.labelLatest.Text = "Latest state";
            // 
            // reconnectTimer
            // 
            this.reconnectTimer.Enabled = true;
            this.reconnectTimer.Interval = 5000;
            this.reconnectTimer.Tick += new System.EventHandler(this.timer_click);
            // 
            // lastTimestamp
            // 
            this.lastTimestamp.AutoSize = true;
            this.lastTimestamp.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lastTimestamp.Location = new System.Drawing.Point(122, 63);
            this.lastTimestamp.Name = "lastTimestamp";
            this.lastTimestamp.Size = new System.Drawing.Size(27, 13);
            this.lastTimestamp.TabIndex = 5;
            this.lastTimestamp.Text = "now";
            // 
            // lastStatus
            // 
            this.lastStatus.BackColor = System.Drawing.SystemColors.Info;
            this.lastStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lastStatus.ForeColor = System.Drawing.SystemColors.InfoText;
            this.lastStatus.Location = new System.Drawing.Point(28, 89);
            this.lastStatus.Name = "lastStatus";
            this.lastStatus.Size = new System.Drawing.Size(337, 34);
            this.lastStatus.TabIndex = 6;
            // 
            // notifyIconServer
            // 
            this.notifyIconServer.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconServer.Icon")));
            this.notifyIconServer.Text = "Nebo Server";
            this.notifyIconServer.Visible = true;
            this.notifyIconServer.Click += new System.EventHandler(this.notifyIconServer_Click);
            // 
            // labelListening
            // 
            this.labelListening.AutoSize = true;
            this.labelListening.Location = new System.Drawing.Point(28, 9);
            this.labelListening.Name = "labelListening";
            this.labelListening.Size = new System.Drawing.Size(67, 13);
            this.labelListening.TabIndex = 7;
            this.labelListening.Text = "Listening on:";
            // 
            // labelDirectory
            // 
            this.labelDirectory.AutoSize = true;
            this.labelDirectory.Location = new System.Drawing.Point(28, 36);
            this.labelDirectory.Name = "labelDirectory";
            this.labelDirectory.Size = new System.Drawing.Size(89, 13);
            this.labelDirectory.TabIndex = 9;
            this.labelDirectory.Text = "Serving directory:";
            // 
            // serverDirectory
            // 
            this.serverDirectory.AutoSize = true;
            this.serverDirectory.ForeColor = System.Drawing.SystemColors.Highlight;
            this.serverDirectory.Location = new System.Drawing.Point(122, 36);
            this.serverDirectory.Name = "serverDirectory";
            this.serverDirectory.Size = new System.Drawing.Size(47, 13);
            this.serverDirectory.TabIndex = 10;
            this.serverDirectory.Text = "directory";
            // 
            // serverLink
            // 
            this.serverLink.AutoSize = true;
            this.serverLink.Location = new System.Drawing.Point(122, 9);
            this.serverLink.Name = "serverLink";
            this.serverLink.Size = new System.Drawing.Size(80, 13);
            this.serverLink.TabIndex = 11;
            this.serverLink.TabStop = true;
            this.serverLink.Text = "http://localhost";
            this.toolTip.SetToolTip(this.serverLink, "\r\nClick here to open Nebo Gauges in your default \r\nbrowser.\r\n\r\n");
            this.serverLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.serverLink_LinkClicked);
            // 
            // toolTip
            // 
            this.toolTip.IsBalloon = true;
            this.toolTip.ToolTipTitle = "Tip";
            // 
            // qrCodeBox
            // 
            this.qrCodeBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.qrCodeBox.Location = new System.Drawing.Point(64, 144);
            this.qrCodeBox.Name = "qrCodeBox";
            this.qrCodeBox.Size = new System.Drawing.Size(256, 256);
            this.qrCodeBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.qrCodeBox.TabIndex = 13;
            this.qrCodeBox.TabStop = false;
            this.toolTip.SetToolTip(this.qrCodeBox, "\r\nYou can use this QR code to open the above link \r\non your device (smartphone, t" +
        "ablet).\r\n\r\nScan this image with the your device\'s camera to \r\nopen the link.");
            // 
            // NeboForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 442);
            this.Controls.Add(this.qrCodeBox);
            this.Controls.Add(this.serverLink);
            this.Controls.Add(this.serverDirectory);
            this.Controls.Add(this.labelDirectory);
            this.Controls.Add(this.labelListening);
            this.Controls.Add(this.lastStatus);
            this.Controls.Add(this.lastTimestamp);
            this.Controls.Add(this.labelLatest);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "NeboForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nebo Server";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.NeboForm_FormClosed);
            this.Load += new System.EventHandler(this.NeboForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.qrCodeBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelLatest;
        private System.Windows.Forms.Timer reconnectTimer;
        private System.Windows.Forms.Label lastTimestamp;
        private System.Windows.Forms.Label lastStatus;
        private System.Windows.Forms.NotifyIcon notifyIconServer;
        private System.Windows.Forms.Label labelListening;
        private System.Windows.Forms.Label labelDirectory;
        private System.Windows.Forms.Label serverDirectory;
        private System.Windows.Forms.LinkLabel serverLink;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.PictureBox qrCodeBox;
    }
}

