using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NoCMD.Extensions;

namespace NoCMD
{
    public sealed class ProcessTrayIcon: IDisposable
    {
        // Todo: rename Notify
        private readonly NotifyIcon _internal;

        public ProcessTrayIcon(Process process)
        {
            _internal = new NotifyIcon
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
            _internal.BalloonTipTitle = title;
            _internal.BalloonTipText = text;
            _internal.BalloonTipIcon = icon;

            _internal.ShowBalloonTip(3000); // 'timeout' not used since Vista
        }

        public string Text
        {
            get { return _internal.Text; }
            set { _internal.Text = value.Truncate(63, "..."); }
        }

        public void AddContextMenuItem(string text, EventHandler onClick)
        {
            _internal.ContextMenu.MenuItems.Add(0, new MenuItem(text, onClick));
        }

        public void Show()
        {
            _internal.Visible = true;
        }

        public void Dispose()
        {
            _internal.Dispose();
        }
    }
}
