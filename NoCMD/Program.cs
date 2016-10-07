using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Text;
using NoCMD.Extensions;

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

        private static void AddTooltip(Process process, ProcessTrayIcon trayIcon, string command)
        {
            var timer = new Timer
            {
                Interval = 1000
            };
            timer.Tick += delegate
            {
                var fullText = "Running: " + (DateTime.Now - process.StartTime).ToString(@"d' days 'hh\:mm\:ss") + "\n" +
                               "Command: " + command;
                trayIcon.Notify.Text = fullText.Truncate(64, "...");
            };

            timer.Start();
        }

        private static void AddErrorBalloon(Process process, ProcessTrayIcon trayIcon)
        {
            process.ErrorDataReceived += (sender, e) => trayIcon.ShowBalloonTip("Error", e.Data, ToolTipIcon.Error);
            try
            {
                process.BeginErrorReadLine();
            }
            catch (InvalidOperationException)
            {
                /* ignored */
            }
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

        private static void AddStandardOutput(Process process, ProcessTrayIcon trayIcon, string fileName)
        {
            var writer = new StreamWriter(fileName) { AutoFlush = true };
            process.OutputDataReceived += (sender, e) => writer.WriteLine(e.Data);

            try
            {
                process.BeginOutputReadLine();
            }
            catch (InvalidOperationException)
            {
                /* ignored */
            }

            var menuItems = trayIcon.Notify.ContextMenu.MenuItems;
            menuItems.Add(0, new MenuItem("Open &output file", delegate { Process.Start(Path.GetFullPath(fileName)); }));
        }

        private static void AddErrorOutput(Process process, ProcessTrayIcon trayIcon, string fileName)
        {
            var writer = new StreamWriter(fileName) { AutoFlush = true };
            process.ErrorDataReceived += (sender, e) => writer.WriteLine(e.Data);

            try
            {
                process.BeginOutputReadLine();
            }
            catch (InvalidOperationException)
            {
                /* ignored */
            }

            var menuItems = trayIcon.Notify.ContextMenu.MenuItems;
            menuItems.Add(0, new MenuItem("Open &error file", delegate { Process.Start(Path.GetFullPath(fileName)); }));
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

                if (config.OutFileName != null) AddStandardOutput(process, icon, config.OutFileName);
                if (config.ErrorFileName != null) AddErrorOutput(process, icon, config.ErrorFileName);
                AddErrorBalloon(process, icon);
                AddTooltip(process, icon, config.Command);

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
