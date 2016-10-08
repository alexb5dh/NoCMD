using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NoCMD.Exceptions;

namespace NoCMD
{
    internal static class Program
    {
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
        private static void Main(string[] args)
        {
            try
            {
                var config = Config.ParseCommandLine(args);

                if (string.IsNullOrEmpty(config.Command)) return; // Todo: add help output

                if (config.WaitForExit)
                    new NoCMDApplication(config).Run();
                else
                    RunWrapper(string.Join(" ", args.Select(arg => "\"" + arg + "\"")));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, $"NoCMD: Internal Error ({e.GetType().Name})");
            }
        }
    }
}
