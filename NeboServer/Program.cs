using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Nebo
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (Environment.GetCommandLineArgs().Contains("--uninstall"))
            {
                Uninstall();
                return;
            }
            int port = Int32.Parse(ConfigurationManager.AppSettings["Port"] ?? "8081");
            string dir = Path.GetFullPath(ConfigurationManager.AppSettings["WWWDir"] ?? "www");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NeboForm(port, dir));
        }

        private static int Execute(string args)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", args)
            {
                Verb = "runas",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            };

            Process proc = Process.Start(psi);
            proc.WaitForExit();
            return proc.ExitCode;
        }

        private static void Uninstall()
        {
            Execute($"advfirewall firewall delete rule name=\"{Application.ProductName}\"");
        }
    }
}