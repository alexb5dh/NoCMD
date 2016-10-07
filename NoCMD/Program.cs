using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Text;

namespace NoCMD
{
    class Program
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private static bool IsConsoleAttached()
        {
            return GetConsoleWindow() != IntPtr.Zero;
        }

        private static void AddRunningTimeDisplay(Process process, ProcessTrayIcon trayIcon)
        {
            //var watch = new Stopwatch();
            //watch.Start();

            var timer = new Timer
            {
                Interval = 1000
            };
            timer.Tick +=
                delegate { trayIcon.Notify.Text = "Running: " + (DateTime.Now - process.StartTime).ToString(@"d' days 'hh\:mm\:ss"); };

            timer.Start();
        }

        private static void AddCommandDisplay(Process process, ProcessTrayIcon trayIcon, string command)
        {
            trayIcon.Notify.BalloonTipTitle = "Command";
            trayIcon.Notify.BalloonTipText = command;
            trayIcon.Notify.BalloonTipIcon = ToolTipIcon.Info;

            trayIcon.Notify.MouseClick += (obj, e) =>
            {
                if (e.Button == MouseButtons.Left) trayIcon.Notify.ShowBalloonTip(0);
            };
        }

        private static void RunApplication(Process process, ProcessTrayIcon trayIcon)
        {
            process.EnableRaisingEvents = true;
            process.Exited += delegate
            {
                trayIcon.Notify.Dispose();
                Application.Exit();
            };

            trayIcon.Show();
            Application.Run();
        }

        // Todo: simplify AddStandartOutput
        private static void AddStandartOutput(Process process, ProcessTrayIcon trayIcon, string fileName)
        {
            var writer = new StreamWriter(fileName, true) { AutoFlush = true };
            process.OutputDataReceived += (sender, e) => writer.WriteLine(e.Data);
            process.BeginOutputReadLine();

            var menuItems = trayIcon.Notify.ContextMenu.MenuItems;
            menuItems.Add(0, new MenuItem("Open &output", delegate { Process.Start(Path.GetFullPath(fileName)); }));
        }

        private static void AddErrorOutput(Process process, ProcessTrayIcon trayIcon)
        {
            var errorBuilder = new StringBuilder();

            process.ErrorDataReceived += (sender, e) => errorBuilder.Append(e.Data);
            process.BeginErrorReadLine();

            process.Exited += delegate
            {
                var error = errorBuilder.ToString();
                if (!string.IsNullOrEmpty(error)) MessageBox.Show(error, "NoCMD - Error");
            };
        }

        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                var config = Config.ParseCommandLine(args);

                if (!config.Wait)
                {
                    Console.WriteLine(string.Join(", ", args));
                    Console.ReadLine();
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Assembly.GetEntryAssembly().Location,
                        Arguments = "/wait " + "\"" + config.Command + "\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                    return 0;
                }

                var processInfo = new ProcessStartInfo(
                    Environment.ExpandEnvironmentVariables("%comspec%"),
                    "/s /c " + "\"" + config.Command + "\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                var process = Process.Start(processInfo);

                var icon = new ProcessTrayIcon(process);

                if (config.OutFileName != null) AddStandartOutput(process, icon, config.OutFileName);
                AddErrorOutput(process, icon);
                AddRunningTimeDisplay(process, icon);
                AddCommandDisplay(process, icon, config.Command);

                RunApplication(process, icon);

                return process.ExitCode;
            }
            catch (Exception e)
            {
                if (IsConsoleAttached()) Console.Error.WriteLine(e.Message);
                else MessageBox.Show(e.Message, "NoCMD: " + e.GetType());
                return -1;
            }
        }
    }
}
