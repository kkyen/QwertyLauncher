using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace QwertyLauncher.Views
{

    internal class TaskTrayIcon
    {
        internal static NotifyIcon TrayIcon;
        private static readonly ContextMenuStrip _notifyMenu = new ContextMenuStrip();

        private static System.Timers.Timer _AnimationTimer = new System.Timers.Timer();
        private static Stopwatch _AnimationTicker = new Stopwatch();
        private static int _AnimationFrame;
        private static string _AnimationType;


        internal TaskTrayIcon()
        {
            TrayIcon = new NotifyIcon
            {
                Visible = true,
                Text = App.Name,
                ContextMenuStrip = _notifyMenu,

            };
            TrayIcon.MouseClick += new MouseEventHandler(NotifyIcon_Click);
            _notifyMenu.Opening += NotifyMenu_Opening;
            TrayIcon.BalloonTipText = App.Name;
            _notifyMenu.Items.Clear();
            _notifyMenu.Items.Add(App.Current.Resources["String.Config"].ToString(), null, Config_Click);
            _notifyMenu.Items.Add(App.Current.Resources["String.EditConfig"].ToString(), null, EditConfig_Click);
            _notifyMenu.Items.Add(App.Current.Resources["String.Exit"].ToString(), null, Exit_Click);
            TrayIcon.Icon = App.IconNormal;

            _AnimationTimer.Interval = 10;
            _AnimationTimer.Elapsed += TaskTrayAnimation_OnTickEvent;

        }

        private void NotifyMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.State != "ready")
            {
                e.Cancel = true;
            }
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

        // Animation
        public static void AnimationStart(string type)
        {
            _AnimationType = type;
            _AnimationTimer.Start();
            _AnimationTicker.Start();
        }
        public static void AnimationStop()
        {
            _AnimationTimer.Stop();
            _AnimationTicker.Reset();
            TrayIcon.Icon = App.IconNormal;
        }

        private void TaskTrayAnimation_OnTickEvent(object sender, EventArgs e)
        {
            var i = (int)Math.Round(CubicInOut(_AnimationTicker.ElapsedMilliseconds, 600, 0, 15)) % 15;
            if (_AnimationFrame != i)
            {
                if(_AnimationType == "Active")
                {
                    TrayIcon.Icon = App.IconActiveAnimation[i];
                } else if(_AnimationType == "Exec")
                {
                    TrayIcon.Icon = App.IconExecAnimation[i];
                }
                else if (_AnimationType == "Recording")
                {
                    TrayIcon.Icon = App.IconRecordingAnimation[i];
                }
                _AnimationFrame = i;
            }
            if (_AnimationTicker.ElapsedMilliseconds > 600) _AnimationTicker.Restart();


        }

        // アニメーション用Easing
        public static float CubicInOut(float t, float totaltime, float min, float max)
        {
            max -= min;
            t /= totaltime / 2;
            if (t < 1) return max / 2 * t * t * t + min;

            t -= 2;
            return max / 2 * (t * t * t + 2) + min;
        }
    }
}
