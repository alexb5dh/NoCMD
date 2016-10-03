using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace NoCMD
{
    public class ProcessTrayIcon
    {
        // Todo: rename Notify
        public readonly NotifyIcon Notify;

        public ProcessTrayIcon(Process process)
        {
            Notify = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(process.MainModule.FileName),
                ContextMenu = new ContextMenu
                {
                    MenuItems =
                    {
                        new MenuItem("E&xit", delegate
                        {
                            try
                            {
                                if (process.MainWindowHandle != IntPtr.Zero) process.CloseMainWindow();
                                else process.Kill();
                            }
                            catch (InvalidOperationException) {}
                        })
                    }
                }
            };
        }

        public void Show()
        {
            Notify.Visible = true;
        }
    }
}
