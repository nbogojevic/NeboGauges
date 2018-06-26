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
            this.label1 = new System.Windows.Forms.Label();
            this.reconnectTimer = new System.Windows.Forms.Timer(this.components);
            this.lastTimestamp = new System.Windows.Forms.Label();
            this.lastStatus = new System.Windows.Forms.Label();
            this.notifyIconServer = new System.Windows.Forms.NotifyIcon(this.components);
            this.labelListening = new System.Windows.Forms.Label();
            this.labelPort = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.serverDirectory = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Latest state";
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
            this.lastStatus.Size = new System.Drawing.Size(337, 139);
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
            this.labelListening.Size = new System.Drawing.Size(88, 13);
            this.labelListening.TabIndex = 7;
            this.labelListening.Text = "Listening on port:";
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.ForeColor = System.Drawing.SystemColors.Highlight;
            this.labelPort.Location = new System.Drawing.Point(122, 9);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(34, 13);
            this.labelPort.TabIndex = 8;
            this.labelPort.Text = "-8081";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Serving directory:";
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
            // NeboForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 256);
            this.Controls.Add(this.serverDirectory);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelPort);
            this.Controls.Add(this.labelListening);
            this.Controls.Add(this.lastStatus);
            this.Controls.Add(this.lastTimestamp);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "NeboForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nebo Server";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.NeboForm_FormClosed);
            this.Load += new System.EventHandler(this.NeboForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer reconnectTimer;
        private System.Windows.Forms.Label lastTimestamp;
        private System.Windows.Forms.Label lastStatus;
        private System.Windows.Forms.NotifyIcon notifyIconServer;
        private System.Windows.Forms.Label labelListening;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label serverDirectory;
    }
}

