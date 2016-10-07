using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;
using NoCMD.Exceptions;

namespace NoCMD
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private static bool IsConsoleAttached()
        {
            return GetConsoleWindow() != IntPtr.Zero;
        }

        private static void RunWrapper(Config config)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Assembly.GetEntryAssembly().Location,
                Arguments = "/wait " + "\"" + config.Command + "\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }

        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                var config = Config.ParseCommandLine(args);

                if (config.Wait)
                {
                    var app = new NoCMDApplication(config);
                    app.Run();
                }

                else
                {
                    RunWrapper(config);
                }

                return 0;
            }
            catch (Exception e)
            {
                if (IsConsoleAttached()) Console.Error.WriteLine(e.Message);
                else MessageBox.Show(e.Message, "NoCMD: " + e.GetType());
                return (e as NonzeroProcessExitCodeException)?.ExitCode ?? -1;
            }
        }
    }
}
