using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
                Text = Path.GetFileNameWithoutExtension(process.MainModule.ModuleName),
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

        public void ShowBalloonTip(string title, string text, ToolTipIcon icon)
        {
            Notify.BalloonTipTitle = title;
            Notify.BalloonTipText = text;
            Notify.BalloonTipIcon = icon;

            Notify.ShowBalloonTip(3000); // 'timeout' not used since Vista
        }

        public void Show()
        {
            Notify.Visible = true;
        }
    }
}
