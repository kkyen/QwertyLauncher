using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using static QwertyLauncher.InputHook;
using System.Globalization;
using System.Reflection;
using QwertyLauncher.Views;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net;

namespace QwertyLauncher
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>

    public partial class App : System.Windows.Application
    {
        public static string Name = "QwertyLauncher";
        public static string Version = "1.3.0";


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
                switch(e.Args[0])
                {
                    case "add":
                    case "action":
                        if (!_createdNew)
                        { 
                            Ipc.SendToMainProcess(e.Args);
                        }
                        Shutdown();
                        return;

                    case "replace":
                        AutoUpdate_Replace(e.Args[1], int.Parse(e.Args[2]));
                        Shutdown();
                        return;

                    case "updateCleanup":
                        AutoUpdate_Cleanup(e.Args[1], int.Parse(e.Args[2]));
                        if (!_createdNew)
                        {
                            _mutex.Close();
                            _mutex = new Mutex(true, Name, out _createdNew);
                        }
                        break;
                }
            }
            if (!_createdNew)
            {
                Shutdown();
                return;
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
                Debug.Print(e.args.ToString());
                switch (e.args[0])
                {
                    case "add":
                        Context.NewKey = new Key(Context);
                        Context.NewKey.Name = Path.GetFileNameWithoutExtension(e.args[1]);
                        Context.NewKey.Path = e.args[1];
                        TaskTrayIcon.TrayIcon.BalloonTipText = Current.Resources["String.RegisterTooltip"].ToString();
                        TaskTrayIcon.TrayIcon.ShowBalloonTip(3);
                        Activate();
                        break;

                    case "action":
                        if (Context.Maps.ContainsKey(e.args[1]))
                        {
                            if (typeof(Map).GetProperties().Any(p => p.Name == e.args[2]))
                            {
                                Context.Maps[e.args[1]][e.args[2]][e.args[3]].Action();
                            }
                        }
                        break;

                    case "shutdown":
                        Shutdown();
                        break;
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


        /// <summary>
        /// メインウィンドウの表示
        /// </summary>
        internal static void Activate()
        {
            if (State == "ready")
            {
                if (CheckDialog())
                {
                    Context.CurrentMapName = "Root";
                    Context.CurrentMod = "default";
                    Context.MainWindowVisibility = Visibility.Visible;
                    MainView.SetKeyAreaFocus();
                }
            }
        }

        internal static void OpenConfigDialog()
        {
            if (State == "ready")
            {
                ConfigWindow window = new ConfigWindow(Context);
                window.ShowDialog();
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
        /// 状態管理
        /// </summary>
        private static string _State = "ready"; // ready, active, configDialog, editDialog, macroRecording, macroPlaying
        internal static string State
        {
            get => _State;
            set {
                switch (value)
                {
                    case "ready":
                    case "macroRecordingReady":
                    case "configDialog":
                    case "editDialog":
                        TaskTrayIcon.AnimationStop();
                        break;
                    case "active":
                        TaskTrayIcon.AnimationStart("Active");
                        break;
                    case "macroRecording":
                        TaskTrayIcon.AnimationStart("Recording");
                        break;
                    case "macroPlaying":
                        TaskTrayIcon.AnimationStart("Exec");
                        break;
                }
                _State = value;
            } 
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
            switch (State)
            {
                case "macroRecordingReady":
                    if (e.Msg == "KEYDOWN")
                    {
                        MacroRecordExitKey = e.Key;
                        MacroRecordStartTime = e.Time;
                        State = "macroRecording";
                        e.Handled = true;
                    }
                    break;

                case "macroRecording":
                    if (e.Key == MacroRecordExitKey)
                    {
                        switch(e.Msg)
                        {
                            case "KEYDOWN":
                                MainView.EditView.Visibility = Visibility.Visible;
                                MainView.EditView.Macro = MacroRecord;
                                State = "editDialog";
                                e.Handled = true;
                                break;
                        }
                    }
                    else
                    {
                        int ms = e.Time - MacroRecordStartTime;
                        MacroRecord += $"{ms},KEYBOARD,{e.Msg},{e.Key}\r\n";
                    }
                    break;

                case "macroPlaying":
                    switch (e.Msg)
                    {
                        case "KEYDOWN":
                            switch (e.Key)
                            {
                                case "Escape":
                                    InputMacro.Cancel();
                                    break;
                            }
                            break;
                    }
                    break;

                case "active":
                    switch (e.Msg)
                    {
                        case "KEYDOWN":


                            switch (e.Key)
                            {
                                case "Escape":
                                    Context.MainWindowVisibility = Visibility.Collapsed;
                                    e.Handled = true;
                                    break;

                                case "Back":
                                    Context.PrevMap();
                                    break;

                                case "Space":
                                    Task.Run(() => Process.Start("notepad.exe"));
                                    Context.MainWindowVisibility = Visibility.Collapsed;
                                    e.Handled = true;
                                    break;

                                default:
                                    MainView.SetKeyFocus(e.Key);
                                    Task.Run(() => Context.CurrentMap[e.Key].Action());
                                    e.Handled = true;
                                    break;

                            }
                            break;

                        case "KEYUP":

                            switch (e.Key)
                            {
                                case "LWin":
                                    Context.MainWindowVisibility = Visibility.Collapsed;
                                    break;

                                case "Up":
                                case "Left":
                                case "PageUp":
                                    Context.MapShift(-1);
                                    break;

                                case "Down":
                                case "Right":
                                case "PageDown":
                                    Context.MapShift(1);
                                    break;

                                default:
                                    MainView.KeyArea.Focus();
                                    break;
                            }
                            break;
                    }
                    break;

                case "ready":
                    switch (e.Msg)
                    {
                        case "KEYUP":
                            int diffTime = e.Time - _prevTime;
                            if (_DoubleClickSpeedMin <= diffTime && diffTime <= Context.DoubleClickSpeed && e.Key == _prevKey)
                            {
                                if (Context.ActivateKeys.Contains(e.Key))
                                {
                                    Activate();
                                }
                            } else {
                                _prevKey = e.Key;
                                _prevTime = e.Time;
                            }
                            break;
                    }
                    
                    break;
                    
            }
            switch (e.Msg)
            {
                case "KEYDOWN":
                    if (Context.ModKeys.Contains(e.Key))
                    {
                        Context.AddCurrentMod(e.Key);
                    }
                    if (Context.CurrentMod != "default")
                    {
                        if (Context.Maps["Root"].Mods.ContainsKey(Context.CurrentMod)) 
                        {
                            Context.Maps["Root"][Context.CurrentMod][e.Key].Action();
                        }
                    }
                    break;

                case "KEYUP":
                    if (Context.ModKeys.Contains(e.Key))
                    {
                        Context.RemoveCurrentMod(e.Key);
                    }
                    break;
            }
            switch (e.Key) {
                case "Scroll":
                case "NumLock":
                    e.Handled = false;
                    break;

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
            switch (State)
            {
                case "macroRecording":
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
                    break;

                case "active":
                    switch (e.Msg)
                    {
                        case "LEFTUP":
                            if (Process.GetCurrentProcess().Id != e.ForegroundWindowId)
                            {
                                Context.MainWindowVisibility = Visibility.Collapsed;
                            }
                            break;

                        case "WHEEL":
                            if (e.Data > 0) Context.MapShift(1);
                            if (e.Data < 0) Context.MapShift(-1);
                            e.Handled = true;
                            break;
                    }
                    break;

                case "ready":
                    switch (e.Msg)
                    {
                        case "LEFTUP":
                            //Debug.Print($"{e.PosX}:{e.PosY}");
                            int diffTime = e.Time - _prevTime;
                            // タスクバーをダブルクリック
                            if (Context.ActivateWithTaskbarDoubleClick)
                            {
                                if (Process.GetProcessById(e.ForegroundWindowId).ProcessName == "explorer")
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
                            break;
                    }
                    break;

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

        internal static int MacroRecordStartTime;
        internal static string MacroRecordExitKey;
        internal static string MacroRecord;

        /// <summary>
        /// マクロ記録開始
        /// </summary>
        internal static void StartMacroRecord()
        {

            TaskTrayIcon.TrayIcon.BalloonTipText = Current.Resources["String.MacroRecordDescription"].ToString();
            TaskTrayIcon.TrayIcon.ShowBalloonTip(3);
            MacroRecord = null;
            MacroRecordExitKey = null;
            MacroRecordStartTime = 0;
            MainView.EditView.Visibility = Visibility.Collapsed;
            State = "macroRecordingReady";
        }

        /// <summary>
        /// 自動更新
        /// </summary>
        private async void AutoUpdate()
        {
            if (!Context.AutoUpdate) { return; }
            // GITのリリースページから最新バージョンを取得
            string url = "https://api.github.com/repos/kkyen/QwertyLauncher/releases/latest";
            WebClient client = new WebClient();
            client.Headers.Add("User-Agent", "QwertyLauncher");

            string json = await Task.Run(() =>
            {
                return client.DownloadString(url);
            });

            dynamic release = System.Text.Json.JsonDocument.Parse(json).RootElement;
            string latestVersion = release.GetProperty("tag_name").GetString();
            string latestUrl = release.GetProperty("assets")[0].GetProperty("browser_download_url").GetString();

            // 最新バージョンと現在のバージョンを比較 
            if (latestVersion == Version) return;

            var newFile = Path.Combine(Location, "QwertyLauncher_" + latestVersion + ".exe");
            if (File.Exists(newFile)) { File.Delete(newFile); }

            await Task.Run(() =>
            {
                client.DownloadFile(latestUrl, newFile);
            });
                    
            var oldFile = Assembly.GetEntryAssembly().Location;
            var oldProsessId = Process.GetCurrentProcess().Id.ToString();
            var args = string.Join(" ",new string[] { "replace", oldFile, oldProsessId });
            _ = Task.Run(() => Process.Start(newFile, args));
            Shutdown();
        }

        /// <summary>
        /// 自動更新のファイル置換処理
        /// </summary>
        private void AutoUpdate_Replace(string oldFile, int oldPid)
        {
            var newFile = Assembly.GetEntryAssembly().Location;
            try
            {
                Process.GetProcessById(oldPid).WaitForExit();
            }
            catch { }
            File.Copy(newFile, oldFile, true);

            var pid = Process.GetCurrentProcess().Id.ToString();
            var args = string.Join(" ", new string[] { "updateCleanup", newFile, pid });

            Task.Run(() => Process.Start(oldFile, args));
        }

        /// <summary>
        /// 自動更新のクリーンアップ処理
        /// </summary>
        private void AutoUpdate_Cleanup(string oldFile, int oldPid)
        {
            try
            {
                Process.GetProcessById(oldPid).WaitForExit();
            }
            catch { }
            while (File.Exists(oldFile))
            {
                try
                {
                    File.Delete(oldFile);
                    break;
                }
                catch
                { }
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