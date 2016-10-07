using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using NoCMD.Exceptions;

namespace NoCMD
{
    public sealed class NoCMDApplication
    {
        private readonly ProcessStartInfo _startInfo;

        private readonly IList<Action<Process, ProcessTrayIcon>> _runConfigurations = new List<Action<Process, ProcessTrayIcon>>();

        private static void AddTooltip(Process process, ProcessTrayIcon trayIcon, string command)
        {
            var timer = new Timer
            {
                Interval = 1000
            };
            timer.Tick += delegate
            {
                trayIcon.Text = "Running: " + (DateTime.Now - process.StartTime).ToString(@"d' days 'hh\:mm\:ss") + "\n" +
                                "Command: " + command;
            };

            process.EnableRaisingEvents = true;
            process.Exited += delegate { timer.Dispose(); };

            timer.Start();
        }

        private static void AddErrorBalloon(Process process, ProcessTrayIcon trayIcon)
        {
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data)) trayIcon.ShowBalloonTip("Error", e.Data, ToolTipIcon.Error);
            };

            try
            {
                process.BeginErrorReadLine();
            }
            catch (InvalidOperationException)
            {
                /* ignored */
            }
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

            trayIcon.AddContextMenuItem("Open &output file", delegate { Process.Start(Path.GetFullPath(fileName)); });
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

            trayIcon.AddContextMenuItem("Open &error file", delegate { Process.Start(Path.GetFullPath(fileName)); });
        }

        public NoCMDApplication(Config config)
        {
            _startInfo = new ProcessStartInfo(
                Environment.ExpandEnvironmentVariables("%comspec%"),
                "/s /c " + "\"" + config.Command + "\"")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            _runConfigurations.Add((process, icon) => AddTooltip(process, icon, config.Command));
            _runConfigurations.Add((process, icon) => AddErrorBalloon(process, icon));

            if (config.OutFileName != null)
                _runConfigurations.Add((process, icon) => AddStandardOutput(process, icon, config.OutFileName));
            if (config.ErrorFileName != null)
                _runConfigurations.Add((process, icon) => AddErrorOutput(process, icon, config.ErrorFileName));
        }

        public void Run()
        {
            var process = Process.Start(_startInfo);
            var icon = new ProcessTrayIcon(process);

            foreach (var configuration in _runConfigurations)
                configuration(process, icon);

            process.EnableRaisingEvents = true;
            process.Exited += delegate
            {
                icon.Dispose();
                Application.Exit();
            };

            icon.Show();
            Application.Run();

            if (process.ExitCode != 0) throw new NonzeroProcessExitCodeException(process.ExitCode);
        }
    }
}
