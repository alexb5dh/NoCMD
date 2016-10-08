using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using NoCMD.Exceptions;

namespace NoCMD
{
    // Todo: implement Dispose for Application
    public sealed class NoCMDApplication : IMessageFilter
    {
        private readonly Process _process;

        private readonly ProcessTrayIcon _icon;

        private bool _running;

        private void AddTooltip(Process process, ProcessTrayIcon trayIcon, string command)
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

            Starting += delegate
            {
                timer.Start();
            };

            Exiting += delegate
            {
                timer.Stop();
                timer.Dispose();
            };
        }

        private void AddErrorBalloon(Process process, ProcessTrayIcon trayIcon)
        {
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data)) trayIcon.ShowBalloonTip("NoCMD", e.Data, ToolTipIcon.Error);
            };
        }

        private void AddExitCodeBalloon(Process process, ProcessTrayIcon trayIcon)
        {
            process.Exited += delegate
            {
                trayIcon.ShowBalloonTip("NoCMD: Exit code", process.ExitCode.ToString(),
                    process.ExitCode == 0 ? ToolTipIcon.Info : ToolTipIcon.Error);
            };
        }

        private void AddStandardOutput(Process process, ProcessTrayIcon trayIcon, string fileName)
        {
            var writer = new StreamWriter(fileName) { AutoFlush = true };
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    writer.WriteLine(e.Data);
            };

            Exiting += delegate { writer.Dispose(); };

            trayIcon.AddContextMenuItem("Open &output file", delegate { Process.Start(Path.GetFullPath(fileName)); });
        }

        private void AddErrorOutput(Process process, ProcessTrayIcon trayIcon, string fileName)
        {
            var writer = new StreamWriter(fileName) { AutoFlush = true };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    writer.WriteLine(e.Data);
            };

            Exiting += delegate { writer.Dispose(); };

            trayIcon.AddContextMenuItem("Open &error file", delegate { Process.Start(Path.GetFullPath(fileName)); });
        }

        private void OnStarting()
        {
            Starting?.Invoke(this, EventArgs.Empty);

            _icon.Show();
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            Application.Run();
        }

        public event EventHandler Starting;

        private void OnExiting()
        {
            Exiting?.Invoke(this, EventArgs.Empty);

            _icon.Hide();
            Application.Exit();
        }

        public event EventHandler Exiting;

        bool IMessageFilter.PreFilterMessage(ref Message m)
        {
            if (!_running) OnExiting();
            return false;
        }

        public NoCMDApplication(Config config)
        {
            Application.AddMessageFilter(this);

            var startInfo = new ProcessStartInfo(
                Environment.ExpandEnvironmentVariables("%comspec%"),
                "/s /c " + "\"" + config.Command + "\"")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            _process = new Process { StartInfo = startInfo };
            _icon = new ProcessTrayIcon(_process, _process.StartInfo);

            AddTooltip(_process, _icon, config.Command);
            AddErrorBalloon(_process, _icon);
            AddExitCodeBalloon(_process, _icon);
            if (config.OutFileName != null)
                AddStandardOutput(_process, _icon, config.OutFileName);
            if (config.ErrorFileName != null)
                AddErrorOutput(_process, _icon, config.ErrorFileName);
        }

        public void Run()
        {
            _process.EnableRaisingEvents = true;
            _process.Exited += async delegate
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                _running = false;
            };

            _running = true;
            OnStarting();
        }
    }
}
