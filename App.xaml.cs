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
using System.Net;
using MessageBox = System.Windows.MessageBox;

namespace QwertyLauncher
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>

    public partial class App : System.Windows.Application
    {
        public static string Name = "QwertyLauncher";
        public static string Version = "1.1.0";


        internal static string Location = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        internal static string ConfigPath = Path.Combine(Location, "config.json");

        private static Mutex _mutex;
        private static bool _createdNew;

        private Ipc _ipc;

        internal static InputHook InputHook;
        internal static InputMacro InputMacro;

        internal static TaskTrayIcon TaskTrayIcon;
        internal static Icon IconNormal;
        internal static Icon[] IconActiveAnimation;
        internal static Icon[] IconExecAnimation;
        internal static Icon[] IconRecordingAnimation;

        internal static MainWindow MainView;
        internal static ViewModel Context;

        internal static FileSystemWatcher WatchConfig;


        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;

            _mutex = new Mutex(true, Name, out _createdNew);
            if (e.Args.Length > 0)
            {
                var method = e.Args[0];
                if (method == "add" && !_createdNew)
                {
                    Ipc.SendToMainProcess(e.Args);
                    Shutdown();
                    return;
                }
                if (method == "replace")
                {
                    var oldFile = e.Args[1];
                    var oldProsessId = e.Args[2];
                    var newFile = Assembly.GetEntryAssembly().Location;

                    try
                    {
                        Process.GetProcessById(int.Parse(oldProsessId)).WaitForExit();
                    }
                    catch { }
                    File.Copy(newFile, oldFile, true);

                    oldFile = Assembly.GetEntryAssembly().Location;
                    oldProsessId = Process.GetCurrentProcess().Id.ToString();
                    var args = string.Join(" ", new string[] { "updated", oldFile, oldProsessId });

                    Task.Run(() => Process.Start(newFile, args));
                    Shutdown();
                    return;
                }
                if (method == "updated")
                {
                    var oldFile = e.Args[1];
                    var oldProsessId = e.Args[2];
                    try
                    {
                        Process.GetProcessById(int.Parse(oldProsessId)).WaitForExit();
                    }
                    catch { }
                    File.Delete(oldFile);
                    if (!_createdNew)
                    {
                        while (!_mutex.WaitOne(1000)) ;
                    }
                }
            }
            else
            {
                if (!_createdNew)
                {
                    Shutdown();
                    return;
                }
            }

            base.OnStartup(e);

            InitializeContext();
            InitializeMainView();
            InitializeTaskTrayIcon();
            InitializeInputHooks();
            InitializeFileSystemWatcher();
            InitializeIpcServer();
            RegisterContextMenu();
            AutoUpdate();
        }

        /// <summary>
        /// ViewModelの初期化
        /// </summary>
        private void InitializeContext()
        {
            Context = new ViewModel();
            Context.PropertyChanged += Context_PropertyChanged;
            ChangeTheme();
        }

        /// <summary>
        /// メインウィンドウの初期化
        /// </summary>
        private void InitializeMainView()
        {
            MainView = new MainWindow(Context);
            MainView.Show();
        }

        /// <summary>
        /// タスクトレイアイコンの初期化
        /// </summary>
        private void InitializeTaskTrayIcon()
        {
            TaskTrayIcon = new TaskTrayIcon();
            TaskTrayIcon.OnExitClickEvent += TaskTrayIcon_OnExitClickEvent;
        }

        /// <summary>
        /// キーボード、マウスのフックの初期化
        /// </summary>
        private void InitializeInputHooks()
        {
            InputHook = new InputHook();
            InputHook.OnKeyboardHookEvent += InputHook_OnKeyboardHookEvent;
            InputHook.OnMouseHookEvent += InputHook_OnMouseHookEvent;

            InputMacro = new InputMacro();
            InputMacro.OnStartMacroEvent += InputMacro_OnStartMacroEvent;
            InputMacro.OnStopMacroEvent += InputMacro_OnStopMacroEvent;
        }

        /// <summary>
        /// 設定ファイルの監視機能の初期化
        /// </summary>
        private void InitializeFileSystemWatcher()
        {
            WatchConfig = new FileSystemWatcher(Path.GetDirectoryName(ConfigPath))
            {
                Filter = Path.GetFileName(ConfigPath),
                NotifyFilter = NotifyFilters.LastWrite
            };
            WatchConfig.Changed += ExternalConfigChange;
            WatchConfig.EnableRaisingEvents = true;
        }

        /// <summary>
        /// 設定ファイル変更イベント
        /// </summary>
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

        /// <summary>
        /// explorerの右クリックメニューに登録
        /// </summary>
        private void RegisterContextMenu()
        {
            Assembly myAssembly = Assembly.GetEntryAssembly();
            string path = myAssembly.Location;
            foreach (var item in new string[] { "*", "Directory" })
            {
                using (var registryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($@"Software\Classes\{item}\shell\QwertyLauncher"))
                {
                    registryKey.SetValue("", Current.Resources["String.KeyAssign"].ToString());
                    registryKey.SetValue("icon", path);
                }
                using (var registryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($@"Software\Classes\{item}\shell\QwertyLauncher\command"))
                {
                    registryKey.SetValue("", $"\"{path}\" add \"%1\"");
                }
            }
        }
        /// <summary>
        /// explorerの右クリックメニューから削除
        /// </summary>
        private void UnRegisterContextMenu()
        {
            foreach (var item in new string[] { "*", "Directory" })
            {
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\" + item + @"\shell\QwertyLauncher", false);
            }
        }


        /// <summary>
        /// プロセス間通信の初期化
        /// 上記のRegisterContextMenu()で登録したコマンドライン引数を受け取る
        /// </summary>
        private void InitializeIpcServer()
        {
            _ipc = new Ipc();
            _ipc.OnCommandLineEvent += Ipc_OnRecieveEvent;
            _ipc.StartServer();
        }
        /// <summary>
        /// プロセス間の通信イベント
        /// </summary>
        private void Ipc_OnRecieveEvent(object sender, Ipc.CommandLineEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Debug.Print(e.args[0]);
                Debug.Print(e.args[1]);
                if (e.args[0] == "add")
                {
                    Context.NewKey = new ViewModel.Key
                    {
                        Name = Path.GetFileNameWithoutExtension(e.args[1]),
                        Path = e.args[1]
                    };
                    TaskTrayIcon.TrayIcon.BalloonTipText = Current.Resources["String.RegisterTooltip"].ToString();
                    TaskTrayIcon.TrayIcon.ShowBalloonTip(3);
                    Activate();
                }
            });
        }

        /// トレイメニュー 終了
        private void TaskTrayIcon_OnExitClickEvent(object sender, EventArgs e)
        {
            Shutdown();
        }

        /// 終了イベント
        protected override void OnExit(ExitEventArgs e)
        {
            if (_createdNew)
            {
                UnRegisterContextMenu();
                TaskTrayIcon.Dispose();
                _mutex.ReleaseMutex();
            }
            _mutex.Close();
        }

        /// メインウィンドウの状態をトレイアイコンに反映
        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "MainWindowVisibility")
            {
                if (Context.MainWindowVisibility == Visibility.Visible) 
                {
                    TaskTrayIcon.AnimationStart("Active");
                };
                if (Context.MainWindowVisibility == Visibility.Collapsed)
                {
                    TaskTrayIcon.AnimationStop();
                }
            }
        }

        /// <summary>
        /// メインウィンドウの表示
        /// </summary>
        internal static void Activate()
        {
            if (!Context.IsActive)
            {
                if (CheckDialog())
                {
                    Context.CurrentMapName = "Root";
                    Context.MainWindowVisibility = Visibility.Visible;
                    MainView.SetKeyAreaFocus();
                }
            }
        }

        /// <summary>
        /// テーマの変更
        /// </summary>
        internal static void ChangeTheme()
        {
            Current.Resources.MergedDictionaries.Clear();
            var iconTheme = "light";
            if (
                !IsLightTheme && Context.Theme == "auto" ||
                Context.Theme == "dark" ||
                Context.Theme == "custom" && Context.IconColor == "dark"
                )
            {
                iconTheme = "dark";
            }

            IconNormal = new Icon(GetResourceStream(new Uri("/Resources/" + iconTheme + "_normal.ico", UriKind.Relative)).Stream);
            IconActiveAnimation = LoadIcons(iconTheme, "active");
            IconExecAnimation = LoadIcons(iconTheme, "exec");
            IconRecordingAnimation = LoadIcons(iconTheme, "Recording");

            Current.Resources.MergedDictionaries.Add(new ResourceDictionary{
                Source = new Uri("/Resources/Template.xaml", UriKind.Relative)
            });
            if (TaskTrayIcon.TrayIcon != null) TaskTrayIcon.ChangeIcon(IconNormal);

            var lang = (CultureInfo.CurrentCulture.Name == "ja-JP") ? "ja-JP" : "en-US";
            var dictionary = new ResourceDictionary
            {
                Source = new Uri("/Resources/Lang_" + lang + ".xaml",UriKind.Relative)
            };
            Current.Resources.MergedDictionaries.Add(dictionary);
        }

        /// <summary>
        /// 連番のアイコンを読み込む
        /// </summary>
        private static Icon[] LoadIcons(string theme, string type)
        {
            return Enumerable.Range(1, 15)
                .Select(i => new Icon(GetResourceStream(new Uri($"/Resources/{theme}_{type}_{i:D2}.ico", UriKind.Relative)).Stream))
                .ToArray();
        }



        /// <summary>
        /// キーボードイベント
        /// </summary>
        private string _prevKey;
        private int _prevTime = 0;
        private readonly int _DoubleClickSpeedMin = 50;
        private void InputHook_OnKeyboardHookEvent(object sender, KeyboardHookEventArgs e)
        {
            //Debug.WriteLine($"Time={e.Time},Msg={e.Msg},Key={e.Key}");
            if (IsMacroRecording)
            {
                if (null == MacroRecordExitKey)
                {
                    if (e.Msg == "KEYDOWN")
                    {
                        MacroRecordExitKey = e.Key;
                        MacroRecordStartTime = e.Time;
                        e.Handled = true;
                        TaskTrayIcon.AnimationStart("Recording");
                    }
                }
                else if (e.Key == MacroRecordExitKey)
                {
                    if(e.Msg == "KEYDOWN")
                    {
                        StopMacroRecord();
                        TaskTrayIcon.AnimationStop();
                        e.Handled = true;
                    }
                }
                else
                {
                    int ms = e.Time - MacroRecordStartTime;
                    MacroRecord += $"{ms},KEYBOARD,{e.Msg},{e.Key}\r\n";
                }
            }
            else if (InputMacro.IsRunning)
            {
                if (e.Msg == "KEYDOWN")
                {
                    if (e.Key == "Escape") InputMacro.Cancel();
                }
            }
            else if (Context.IsActive)
            {
                if (e.Msg == "KEYDOWN")
                {
                    if (Context.IsKeyAreaFocus)
                    {
                        if (new string[] {
                            "LControlKey",
                            "RControlKey",
                            "LShiftKey",
                            "RShiftKey",
                            "LWin" 
                        }.Contains(e.Key))
                        {
                            e.Handled = false;
                        }
                        else if (new string[] {
                            "Escape",
                            "Back" 
                        }.Contains(e.Key))
                        {
                            Context.MainWindowVisibility = Visibility.Collapsed;
                        }
                        else if (new string[] {
                            "Space"
                        }.Contains(e.Key))
                        {
                            Task.Run(() => Process.Start("notepad.exe"));
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
                if (e.Msg == "KEYUP")
                {
                    if (e.Key == "LWin")
                    {
                        Context.MainWindowVisibility = Visibility.Collapsed;
                    }
                    else if (new string[] {
                        "Up", 
                        "Left",
                        "PageUp" 
                    }.Contains(e.Key))
                    {
                        Context.MapShift(-1);
                    }
                    else if (new string[] {
                        "Down",
                        "Right", 
                        "Next" 
                    }.Contains(e.Key))
                    {
                        Context.MapShift(1);
                    }
                    else
                    {
                        MainView.KeyArea.Focus();
                    }
                }
            }
            else if (!Context.IsDialogOpen)
            {
                if (e.Msg == "KEYUP")
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

        /// <summary>
        /// マウスのイベント
        /// </summary>
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
                        //Debug.Print($"{e.PosX}:{e.PosY}");
                        int diffTime = e.Time - _prevTime;
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
                if(e.Msg == "WHEEL")
                {
                    //Debug.WriteLine($"Time={e.Time},Msg={e.Msg},Data={e.Data}");
                    if (Context.IsActive)
                    {
                        if (e.Data > 0) Context.MapShift(1);
                        if (e.Data < 0) Context.MapShift(-1);
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// プライマリスクリーンの拡大率
        /// </summary>
        public static double GetMagnifyRate()
        {
            double hw = Screen.PrimaryScreen.Bounds.Width;
            double sw = SystemParameters.PrimaryScreenWidth;
            return hw / sw;
        }

        /// <summary>
        /// マクロ開始イベント
        /// </summary>
        private void InputMacro_OnStartMacroEvent(object sender, EventArgs e)
        {
            TaskTrayIcon.AnimationStart("Exec");
        }

        /// <summary>
        /// マクロ終了イベント
        /// </summary>
        private void InputMacro_OnStopMacroEvent(object sender, EventArgs e)
        {
            TaskTrayIcon.AnimationStop();
        }

        /// <summary>
        /// カーソルのあるScreenを返す
        /// </summary>
        /// <returns></returns>
        internal static Screen GetCurrentScreen()
        {
            System.Drawing.Point pos = Cursor.Position;
            return Screen.AllScreens.FirstOrDefault(s =>
                pos.X >= s.Bounds.Left &&
                pos.Y >= s.Bounds.Top &&
                pos.X <= s.Bounds.Right &&
                pos.Y <= s.Bounds.Bottom) ?? Screen.PrimaryScreen;
        }

        /// <summary>
        /// Windowsのテーマがライトかどうか
        /// </summary>
        internal static bool IsLightTheme
        {
            get
            {
                using (var rKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    return Convert.ToBoolean(rKey.GetValue("SystemUsesLightTheme"));
                }
            }
        }

        /// <summary>
        /// ダイヤログの表示状態
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// マクロ記録開始イベント
        /// </summary>
        internal static void StartMacroRecord()
        {

            TaskTrayIcon.TrayIcon.BalloonTipText = Current.Resources["String.MacroRecordDescription"].ToString();
            TaskTrayIcon.TrayIcon.ShowBalloonTip(3);
            MacroRecord = null;
            MacroRecordExitKey = null;
            MacroRecordStartTime = 0;
            IsMacroRecording = true;
            MainView.EditView.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// マクロ記録終了イベント
        /// </summary>
        internal static void StopMacroRecord()
        {
            IsMacroRecording = false;
            MainView.EditView.Visibility = Visibility.Visible;
            MainView.EditView.Macro = MacroRecord;
            TaskTrayIcon.ChangeIcon(IconNormal);
        }

        /// <summary>
        /// 自動更新
        /// </summary>
        private void AutoUpdate()
        {
            if (!Context.AutoUpdate) { return; }
            // GITのリリースページから最新バージョンを取得
            string url = "https://api.github.com/repos/kkyen/QwertyLauncher/releases/latest";
            using (System.Net.WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent", "QwertyLauncher");
                string json = client.DownloadString(url);
                dynamic release = System.Text.Json.JsonDocument.Parse(json).RootElement;
                string latestVersion = release.GetProperty("tag_name").GetString();
                string latestUrl = release.GetProperty("assets")[0].GetProperty("browser_download_url").GetString();

                // 最新バージョンと現在のバージョンを比較 
                if (latestVersion != Version)
                {
                    var newFile = Path.Combine(Location, "QwertyLauncher_" + latestVersion + ".exe");
                    if (File.Exists(newFile)) { File.Delete(newFile); }
                    client.DownloadFile(latestUrl, newFile);
                    
                    var oldFile = Assembly.GetEntryAssembly().Location;
                    var oldProsessId = Process.GetCurrentProcess().Id.ToString();
                    var args = string.Join(" ",new string[] { "replace", oldFile, oldProsessId });
                    Task.Run(() => Process.Start(newFile, args));
                    Shutdown();
                    return;
                }
            }
        }

        /// <summary>
        /// dllをexeに含める
        /// </summary>
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
    }
}