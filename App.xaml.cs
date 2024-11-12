using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using static QwertyLauncher.Views.InputHook;
using System.Globalization;
using System.Reflection;
using QwertyLauncher.Views;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QwertyLauncher
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>

    public partial class App : System.Windows.Application
    {
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            string path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
            {
                path = string.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
            }

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                    return null;

                byte[] assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }
        internal static string Name = "QwertyLauncher";

        internal static string Location = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        internal static string ConfigPath = Path.Combine(Location, "config.json");
        internal static string TempPath = GetRamdomTempDirectory();
        internal static string ThemePath;

        private static Mutex _mutex;
        internal static InputHook InputHook;
        internal static InputMacro InputMacro;
        internal static TaskTrayIcon TaskTrayIcon;
        internal static Icon IconNormal;
        internal static Icon IconActive;
        internal static Icon IconExecute;
        internal static Icon IconRecording;

        internal static MainWindow MainView;
        internal static ViewModel Context;

        internal static FileSystemWatcher WatchConfig;


        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;

            _mutex = new Mutex(true, Name, out bool _createdNew);
            if (_createdNew == false)
            {
                _mutex.Close();
                return;
            }
            base.OnStartup(e);

            Context = new ViewModel();
            Context.PropertyChanged += Context_PropertyChanged;

            ChangeTheme();

            MainView = new MainWindow(Context);

            TaskTrayIcon = new TaskTrayIcon();
            TaskTrayIcon.OnExitClickEvent += TaskTrayIcon_OnExitClickEvent;
            InputHook = new InputHook();
            InputHook.OnKeyboardHookEvent += InputHook_OnKeyboardHookEvent;
            InputHook.OnMouseHookEvent += InputHook_OnMouseHookEvent;
            InputMacro = new InputMacro();
            InputMacro.OnStartMacroEvent += InputMacro_OnStartMacroEvent;
            InputMacro.OnStopMacroEvent += InputMacro_OnStopMacroEvent;
            MainView.Show();

            // Configの外部編集を監視
            WatchConfig = new FileSystemWatcher(Path.GetDirectoryName(ConfigPath))
            {
                Filter = Path.GetFileName(ConfigPath),
                NotifyFilter = NotifyFilters.LastWrite
            };
            WatchConfig.Changed += new FileSystemEventHandler(ExternalConfigChange);
            WatchConfig.EnableRaisingEvents = true;
        }

        private void TaskTrayIcon_OnExitClickEvent(object sender, EventArgs e)
        {
            Shutdown();
        }

        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "MainWindowVisibility")
            {
                if (Context.MainWindowVisibility == Visibility.Visible) TaskTrayIcon.ChangeIcon(IconActive);
                if (Context.MainWindowVisibility == Visibility.Collapsed) TaskTrayIcon.ChangeIcon(IconNormal);
            }
        }

        private static void ExternalConfigChange(Object sender, FileSystemEventArgs e)
        {
            WatchConfig.EnableRaisingEvents = false;
            Thread.Sleep(500);
            MainView.Dispatcher.BeginInvoke(new Action(() =>
            {
                MainView.Close();
                Context = new ViewModel();
                MainView = new MainWindow(Context);
                MainView.Show();
            }));
            WatchConfig.EnableRaisingEvents = true;
        }


        protected override void OnExit(ExitEventArgs e)
        {
            TaskTrayIcon.Dispose();
            _mutex.ReleaseMutex();
            _mutex.Close();
            Directory.Delete(TempPath, true);
        }

        internal static void Activate()
        {
            if (!Context.IsActive)
            {
                if (CheckDialog())
                {
                    Context.CurrentMapName = "default";
                    Context.MainWindowVisibility = Visibility.Visible;
                    MainView.SetKeyAreaFocus();
                }
            }
        }

        internal static void ChangeTheme()
        {
            Current.Resources.MergedDictionaries.Clear();
            ResourceDictionary baseDict = new ResourceDictionary
            {
                Source = new Uri("/Theme/base.xaml", UriKind.Relative)
            };
            Current.Resources.MergedDictionaries.Add(baseDict);

            if (Context.Theme == "auto")
            {
                ThemePath = (IsLightTheme) ? "/Theme/light" : "/Theme/dark";
            } else
            {
                ThemePath = $"/Theme/{Context.Theme}";
            }
            ResourceDictionary themeDict = new ResourceDictionary
            {
                Source = new Uri($"{ThemePath}/theme.xaml", UriKind.Relative)
            };
            Current.Resources.MergedDictionaries.Add(themeDict);
            IconNormal = new Icon(GetResourceStream(new Uri($"{ThemePath}/normal.ico", UriKind.Relative)).Stream);
            IconActive = new Icon(GetResourceStream(new Uri($"{ThemePath}/activate.ico", UriKind.Relative)).Stream);
            IconExecute = new Icon(GetResourceStream(new Uri($"{ThemePath}/execute.ico", UriKind.Relative)).Stream);
            IconRecording = new Icon(GetResourceStream(new Uri($"{ThemePath}/recording.ico", UriKind.Relative)).Stream);

            if (TaskTrayIcon.TrayIcon != null) TaskTrayIcon.ChangeIcon(IconNormal);


            var lang = (CultureInfo.CurrentCulture.Name == "ja-JP") ? "ja-JP" : "en-US";
            var dictionary = new ResourceDictionary
            {
                Source = new Uri("/Lang/" + lang + ".xaml",UriKind.Relative)
            };
            Current.Resources.MergedDictionaries.Add(dictionary);
        }

        private static string GetRamdomTempDirectory()
        {
            string temp = Path.GetTempPath();
            string path;
            do
            {
                path = temp + Path.GetRandomFileName();
            } while (File.Exists(path) || Directory.Exists(path));
            Directory.CreateDirectory(path);
            return path;
        }


        //キーおしっぱ対策
        private readonly int _DoubleClickSpeedMin = 50;

        private string _prevKey;
        private int _prevTime = 0;

        private void InputHook_OnKeyboardHookEvent(object sender, KeyboardHookEventArgs e)
        {
            //Debug.WriteLine($"Time={e.Time},Msg={e.Msg},Key={e.Key}");
            if (IsMacroRecording)
            {
                if(null == MacroRecordExitKey)
                {
                    if(e.Msg == "KEYDOWN") {
                        MacroRecordExitKey = e.Key;
                        MacroRecordStartTime = e.Time;
                        e.Handled = true;
                        TaskTrayIcon.TrayIcon.Icon = IconRecording;
                    }
                }
                else
                {
                    if(e.Key == MacroRecordExitKey)
                    {
                        if(e.Msg == "KEYDOWN")
                        {
                            StopMacroRecord();
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        int ms = e.Time - MacroRecordStartTime;
                        MacroRecord += $"{ms},KEYBOARD,{e.Msg},{e.Key}\r\n";
                    }
                }
            } else
            {
                if (e.Msg == "KEYDOWN")
                {
                    if (InputMacro.IsRunning)
                    {
                        if (e.Key == "Escape") InputMacro.Cancel();
                    }
                    else if (Context.IsActive)
                    {
                        if (Context.IsKeyAreaFocus)
                        {
                            if (
                                e.Key == "LControlKey" ||
                                e.Key == "RControlKey" ||
                                e.Key == "LShiftKey" ||
                                e.Key == "RShiftKey" ||
                                e.Key == "LWin")
                            {
                                e.Handled = false;
                            }
                            else if (e.Key == "Escape")
                            {
                                Context.MainWindowVisibility = Visibility.Collapsed;
                            }
                            else
                            {
                                MainView.SetKeyFocus(e.Key);
                                e.Handled = true;
                                Task.Run(() => Context.KeyAction(e.Key));
                            }
                        }
                    }
                }
                if (e.Msg == "KEYUP")
                {
                    if (Context.IsActive)
                    {
                        if (e.Key == "LWin")
                        {
                            Context.MainWindowVisibility = Visibility.Collapsed;
                        }
                        else
                        {
                            MainView.KeyArea.Focus();
                        }
                    }
                    else if (!Context.IsDialogOpen)
                    {
                        int diffTime = e.Time - _prevTime;

                        if (_DoubleClickSpeedMin <= diffTime && diffTime <= Context.DoubleClickSpeed && e.Key == _prevKey)
                        {
                            if (Context.ActivateKeys.Contains(e.Key))
                            {
                                Activate();
                            }
                        }
                        _prevKey = e.Key;
                        _prevTime = e.Time;
                    }
                }
            }
        }

        private int _prevX;
        private int _prevY;
        private void InputHook_OnMouseHookEvent(object sender, MouseHookEventArgs e)
        {
            //Debug.WriteLine($"Time={e.Time},Msg={e.Msg},PosX={e.PosX},PosY={e.PosY}");
            if (IsMacroRecording)
            {
                if (null != MacroRecordExitKey)
                {
                    int ms = e.Time - MacroRecordStartTime;
                    if (e.Msg == "MOVE")
                    {
                        if (Context.AdvancedMouseRecording)
                        {
                            if (e.PosX != _prevX && e.PosY != _prevY)
                            {
                                MacroRecord += $"{ms},MOUSE,ABSOLUTESTROKE,{e.PosX},{e.PosY}\r\n";
                                _prevX = e.PosX;
                                _prevY = e.PosY;
                            }
                        }
                    }
                    else
                    {
                        if (e.PosX != _prevX && e.PosY != _prevY)
                        {
                            MacroRecord += $"{ms},MOUSE,ABSOLUTESTROKE,{e.PosX},{e.PosY}\r\n";
                            _prevX = e.PosX;
                            _prevY = e.PosY;
                        }
                        MacroRecord += $"{ms},MOUSE,{e.Msg},{e.Data}\r\n";
                    }

                }
            }
            else
            {

                if (e.Msg == "LEFTUP")
                {

                    if (Context.IsActive)
                    {
                        if (Process.GetCurrentProcess().Id != e.ForegroundWindowId)
                        {
                            Context.MainWindowVisibility = Visibility.Collapsed;
                        }
                    }
                    else if (!Context.IsDialogOpen)
                    {
                        int diffTime = e.Time - _prevTime;
                        Debug.Print($"{e.PosX}:{e.PosY}");
                        // タスクバーをダブルクリック
                        if (Context.ActivateWithTaskbarDoubleClick)
                        {
                            if(Process.GetProcessById(e.ForegroundWindowId).ProcessName == "explorer")
                            {
                                if (_DoubleClickSpeedMin <= diffTime && diffTime <= Context.DoubleClickSpeed)
                                {
                                    Screen s = GetCurrentScreen();
                                    double rate = GetMagnifyRate();
                                    if (s.Primary) rate = 1;
                                    int min_x = (int)(s.WorkingArea.X / rate);
                                    int max_x = min_x + (int)(s.WorkingArea.Width / rate);
                                    int min_y = (int)(s.WorkingArea.Y / rate);
                                    int max_y = min_y + (int)(s.WorkingArea.Height / rate);

                                    if (!(min_x <= e.PosX && e.PosX <= max_x && min_y <= e.PosY && e.PosY <= max_y))
                                    {
                                        Activate();
                                    }
                                }
                            }
                        }
                        _prevTime = e.Time;
                    }
                }
            }
        }

        // プライマリスクリーンの拡大率
        public static double GetMagnifyRate()
        {
            double hw = Screen.PrimaryScreen.Bounds.Width;
            double sw = SystemParameters.PrimaryScreenWidth;
            double rate = hw / sw;
            return rate;
        }

        private void InputMacro_OnStartMacroEvent(object sender, EventArgs e)
        {
            TaskTrayIcon.ChangeIcon(IconRecording);
        }
        private void InputMacro_OnStopMacroEvent(object sender, EventArgs e)
        {
            TaskTrayIcon.ChangeIcon(IconNormal);
        }


        // カーソルのあるScreenを返す
        internal static Screen GetCurrentScreen()
        {
            System.Drawing.Point pos = Cursor.Position;
            foreach (Screen s in Screen.AllScreens)
            {
                if (
                    pos.X >= s.Bounds.Left &&
                    pos.Y >= s.Bounds.Top &&
                    pos.X <= s.Bounds.Right &&
                    pos.Y <= s.Bounds.Bottom
                )
                {
                    return s;
                }
            }
            return Screen.PrimaryScreen;
        }
        // WIndowsのテーマがライトかどうか
        internal static bool IsLightTheme
        {
            get
            {
                bool nResult;
                string sKeyName = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
                string sSubkeyName = "SystemUsesLightTheme";
                Microsoft.Win32.RegistryKey rKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(sKeyName);
                nResult = Convert.ToBoolean(rKey.GetValue(sSubkeyName));
                rKey.Close();
                return nResult;
            }
        }
        internal static bool CheckDialog()
        {
            EditWindow editWindow = Current.Windows.OfType<EditWindow>().FirstOrDefault();
            if (editWindow != null)
            {
                SystemCommands.RestoreWindow(editWindow);
                editWindow.Activate();
                return false;
            }
            ConfigWindow configWindow = Current.Windows.OfType<ConfigWindow>().FirstOrDefault();
            if (configWindow != null)
            {
                SystemCommands.RestoreWindow(configWindow);
                configWindow.Activate();
                return false;
            }
            return true;
        }

        internal static bool IsMacroRecording = false;
        internal static int MacroRecordStartTime;
        internal static string MacroRecordExitKey;
        internal static string MacroRecord;

        internal static void StartMacroRecord()
        {

            TaskTrayIcon.TrayIcon.BalloonTipText = App.Current.Resources["String.MacroRecordDescription"].ToString();
            TaskTrayIcon.TrayIcon.ShowBalloonTip(3);
            MacroRecord = null;
            MacroRecordExitKey = null;
            MacroRecordStartTime = 0;
            IsMacroRecording = true;
            App.MainView.EditView.Visibility = Visibility.Collapsed;
        }
        internal static void StopMacroRecord()
        {
            IsMacroRecording = false;
            App.MainView.EditView.Visibility = Visibility.Visible;
            App.MainView.EditView.Macro = MacroRecord;
            TaskTrayIcon.ChangeIcon(IconNormal);
        }

    }
}