using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Nebo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int port = Environment.GetCommandLineArgs().SkipWhile(s => s != "--port").Skip(1).Take(1).Select(s => Int32.Parse(s)).FirstOrDefault();
            if (port == 0)
            {
                port = Int32.Parse(ConfigurationManager.AppSettings["Port"] ?? "8081");
            }
            if (Environment.GetCommandLineArgs().Contains("--uninstall"))
            {
                Uninstall(port);
                return;
            }
            string dir = Path.GetFullPath(ConfigurationManager.AppSettings["WWWDir"] ?? "www");
            if (Environment.GetCommandLineArgs().Contains("--install"))
            {
                if (!AddAddress(port))
                {
                    return;
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            NeboForm mainForm = new NeboForm(port, dir);
            if (mainForm.HasServer)
            {
                Application.Run(mainForm);
            }
            else
            {
                RestartAndInstall(port);
            }
        }

        static int Execute(string args)
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

        static void Uninstall(int port)
        {
            string deleteArgs = $"http delete urlacl url = http://+:{port}/";
            var result = Execute(deleteArgs);
            if (result != 0)
            {
                MessageBox.Show($@"Unable to delete URL reservation. 

Was trying to execute delete reservation using `netsh {deleteArgs}`.

Result code is {result}.");
            }
            Execute($"advfirewall firewall delete rule name=\"{Application.ProductName}\"");

        }

        static bool AddAddress(int port)
        {
            string args = $"http add urlacl url=http://+:{port}/ user={Environment.UserDomainName}\\{Environment.UserName}";

            int result = Execute(args);
            if (result == 183)
            {
                string deleteArgs = $"http delete urlacl url = http://+:{port}/";
                result = Execute(deleteArgs);
                if (result != 0)
                {
                    MessageBox.Show($@"Unable to delete previous URL reservation. 

Was trying to execute delete reservation using `netsh {deleteArgs}`.

Result code is {result}.");
                    return false;
                }
                result = Execute(args);
            }

            if (result != 0)
            {
                MessageBox.Show($@"Unable to add new URL reservation. 

Was trying to execute add new reservation using `netsh {args}`.

Result code is {result}.");
                return false;
            }
            Execute($"advfirewall firewall delete rule name=\"{Application.ProductName}\"");
            string firewallArgs = $"advfirewall firewall add rule name=\"{Application.ProductName}\" dir=in action=allow localip=any remoteip=any localport={port} protocol=tcp";
            result = Execute(firewallArgs);
            if (result != 0)
            {
                MessageBox.Show($@"Unable to add firewall rule for port {port}. 

Was trying to execute add firewall rule using `netsh {firewallArgs}`.;

Result code is { result}.");
                return false;
            }
            return true;
        }

        static void RestartAndInstall(int port)
        {
            ProcessStartInfo proc = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Application.ExecutablePath,
                Arguments = "--install",

                Verb = "runas"
            };
            MessageBox.Show($@"Program was unable to start listening on {port}.

To run correctly, URL http://+:{port}/ must be reserved for the current user ({Environment.UserDomainName}\\{Environment.UserName}), and firewall must allow programs to listen on same port.

The program will now restart and request to run as Administrator in order to apply those changes to your system which and to allow it to run as non-administrator later on.",
                "Network Configuration Needed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);

            try
            {
                Process.Start(proc);
            }
            catch
            {
                return;
            }
        }
    }
}