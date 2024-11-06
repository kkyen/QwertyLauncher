using QwertyLauncher.Views;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;
using System.Linq;

namespace QwertyLauncher.Views
{

    internal class TaskTrayIcon
    {
        internal static NotifyIcon TrayIcon;
        private static readonly ContextMenuStrip _notifyMenu = new ContextMenuStrip();

        internal TaskTrayIcon()
        {
            TrayIcon = new NotifyIcon
            {
                Visible = true,
                Text = App.Name,
                ContextMenuStrip = _notifyMenu,

            };
            TrayIcon.MouseClick += new MouseEventHandler(NotifyIcon_Click);
            _notifyMenu.Items.Clear();
            _notifyMenu.Items.Add(App.Current.Resources["String.Config"].ToString(), null, Config_Click);
            _notifyMenu.Items.Add(App.Current.Resources["String.EditConfig"].ToString(), null, EditConfig_Click);
            _notifyMenu.Items.Add(App.Current.Resources["String.Exit"].ToString(), null, Exit_Click);
            TrayIcon.Icon = App.IconNormal;
        }

        internal static void Dispose()
        {
            TrayIcon.Dispose();
        }
        internal static void ChangeIcon(Icon Icon)
        {
            TrayIcon.Icon = Icon;
        }

        private static void NotifyIcon_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                App.Activate();
            }
        }
        private void Config_Click(object sender, EventArgs e)
        {
            if (App.CheckDialog())
            {
                ConfigWindow window = new ConfigWindow(App.Context);
                window.ShowDialog();
            }

        }

        private void EditConfig_Click(object sender, EventArgs e)
        {
            Process.Start(App.ConfigPath);
        }
        internal event EventHandler OnExitClickEvent = delegate { };
        private void Exit_Click(object sender, EventArgs e)
        {
            //Shutdown();
            OnExitClickEvent(this, EventArgs.Empty);
        }

    }
}
