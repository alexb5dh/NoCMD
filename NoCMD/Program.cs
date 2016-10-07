using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using NoCMD.Exceptions;

namespace NoCMD
{
    internal static class Program
    {
        private static bool IsConsoleAttached()
        {
            var attached = true;
            try { GC.KeepAlive(Console.WindowHeight); }
            catch { attached = false; }
            return attached;
        }

        private static void RunWrapper(string commandLine)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                Arguments = "/wait " + commandLine,
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }

        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                var config = Config.ParseCommandLine(args);

                if (config.WaitForExit)
                {
                    new NoCMDApplication(config).Run();
                }
                else
                {
                    RunWrapper(string.Join(" ", args.Select(arg => "\"" + arg + "\"")));
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
