using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace QwertyLauncher
{
    public class ViewModel : ViewModelBase
    {
        // Constructor
        // **************************************************
        public ViewModel()
        {
            _conf = new Models.Config(App.ConfigPath);
            _Theme = _conf.Theme;
            SetThemeColor();
            if (_conf.CustomTheme.ContainsKey("Foreground")) _Foreground = _conf.CustomTheme["Foreground"];
            if (_conf.CustomTheme.ContainsKey("Background")) _Background = _conf.CustomTheme["Background"];
            if (_conf.CustomTheme.ContainsKey("AccentInfo")) _AccentInfo = _conf.CustomTheme["AccentInfo"];
            if (_conf.CustomTheme.ContainsKey("AccentWarning")) _AccentWarning = _conf.CustomTheme["AccentWarning"];
            if (_conf.CustomTheme.ContainsKey("AccentError")) _AccentError = _conf.CustomTheme["AccentError"];
            if (_conf.CustomTheme.ContainsKey("CornerRadius")) _CornerRadius = _conf.CustomTheme["CornerRadius"];

            _ActivateKeys = _conf.ActivateKeys;
            _ActivateWithTaskbarDoubleClick = _conf.ActivateWithTaskbarDoubleClick;
            _ShowQwerty = _conf.ShowQwerty;
            _ShowFunction = _conf.ShowFunction;
            _ShowNumPad = _conf.ShowNumPad;
            _DoubleClickSpeed = _conf.DoubleClickSpeed;
            _AdvancedMouseRecording = _conf.AdvancedMouseRecording;
            _DownloadFavicon = _conf.DownloadFavicon;

            foreach (string mapname in _conf.Maps.Keys)
            {
                Map map = new Map();
                map.MapUpdateEventHandler += MapUpdate;
                foreach (string keyname in _conf.Maps[mapname].Keys)
                {
                    var item = _conf.Maps[mapname][keyname];
                    Key key = new Key(item);
                    map.GetType().GetProperty(keyname).SetValue(map, key);
                }
                Maps.Add(mapname, map);
            }
            CurrentMap = Maps[CurrentMapName];
        }

        // Properties
        // **************************************************
        // ウィンドウの表示状態
        private Visibility _MainWindowVisibility = Visibility.Collapsed;
        public Visibility MainWindowVisibility
        {
            get => _MainWindowVisibility;
            set
            {
                if (RaisePropertyChangedIfSet(ref _MainWindowVisibility, value))
                {
                    if (value == Visibility.Visible)
                    {
                        IsActive = true;

                        Screen s = App.GetCurrentScreen();
                        double rate = App.GetMagnifyRate();
                        int w = (int)(s.Bounds.Width / rate);
                        int h = (int)(s.Bounds.Height / rate);
                        int wh = (int)(s.WorkingArea.Height / rate);
                        int l = (int)(s.Bounds.Left / rate);
                        int t = (int)(s.Bounds.Top / rate);

                        int width = Math.Max(w, h);
                        int height = Math.Min(w, h) / 2;
                        width = width / 16 * 9;

                        MainWindowLeft = l + ((w - width) / 2);
                        MainWindowTop = t + wh - height;
                        MainWindowWidth = width;
                        MainWindowHeight = height;
                    }
                    else if (value == Visibility.Collapsed)
                    {
                        IsActive = false;
                    }
                }
            }
        }



        private static Models.Config _conf;

        // FLAGS
        public bool IsKeyAreaFocus { get; set; }
        public bool IsActive { get; set; }
        public bool IsDialogOpen { get; set; }

        //ウィンドウの位置
        private int _MainWindowLeft = 0;
        public int MainWindowLeft
        {
            get => _MainWindowLeft;
            set { RaisePropertyChangedIfSet(ref _MainWindowLeft, value); }
        }
        private int _MainWindowTop = 0;
        public int MainWindowTop
        {
            get => _MainWindowTop;
            set { RaisePropertyChangedIfSet(ref _MainWindowTop, value); }
        }
        private int _MainWindowWidth = 0;
        public int MainWindowWidth
        {
            get => _MainWindowWidth;
            set { RaisePropertyChangedIfSet(ref _MainWindowWidth, value); }
        }
        private int _MainWindowHeight = 0;
        public int MainWindowHeight
        {
            get => _MainWindowHeight;
            set { RaisePropertyChangedIfSet(ref _MainWindowHeight, value); }
        }

        private bool _IsChangeMap = false;
        public bool IsChangeMap
        {
            get => _IsChangeMap;
            set { RaisePropertyChangedIfSet(ref _IsChangeMap, value); }
        }




        // ActivateKeys
        private string[] _ActivateKeys;
        public string[] ActivateKeys
        {
            get => _ActivateKeys;
            set
            {
                RaisePropertyChangedIfSet(ref _ActivateKeys, value);
                RaisePropertyChanged("JoinActivateKeys");
                _conf.ActivateKeys = value;
            }
        }
        public string JoinActivateKeys
        {
            get => string.Join(",", _ActivateKeys);
            set
            {
                var array = value.Split(',');
                RaisePropertyChangedIfSet(ref _ActivateKeys, array);
                _conf.ActivateKeys = array;
            }
        }

        // タスクバーをダブルクリック
        private bool _ActivateWithTaskbarDoubleClick;
        public bool ActivateWithTaskbarDoubleClick
        {
            get => _ActivateWithTaskbarDoubleClick;
            set
            {
                RaisePropertyChangedIfSet(ref _ActivateWithTaskbarDoubleClick, value);
                _conf.ActivateWithTaskbarDoubleClick = value;
            }
        }

        // Qwertyの表示
        private bool _ShowQwerty;
        public bool ShowQwerty
        {
            get => _ShowQwerty;
            set {
                RaisePropertyChangedIfSet(ref _ShowQwerty, value);
                _conf.ShowQwerty = value;
            }
        }
        // ファンクションキーの表示
        private bool _ShowFunction;
        public bool ShowFunction
        {
            get => _ShowFunction;
            set {
                RaisePropertyChangedIfSet(ref _ShowFunction, value);
                _conf.ShowFunction = value;
            }
        }
        // テンキーの表示
        private bool _ShowNumPad;
        public bool ShowNumPad
        {
            get => _ShowNumPad;
            set {
                RaisePropertyChangedIfSet(ref _ShowNumPad, value);
                _conf.ShowNumPad = value;
            }
        }
        // ダブルクリック速度
        private int _DoubleClickSpeed ;
        public int DoubleClickSpeed
        {
            get => _DoubleClickSpeed;
            set {
                RaisePropertyChangedIfSet(ref _DoubleClickSpeed, value);
                _conf.DoubleClickSpeed = value;
            }
        }
        // 高度なマウス記録
        private bool _AdvancedMouseRecording;
        public bool AdvancedMouseRecording
        {
            get => _AdvancedMouseRecording;
            set
            {
                RaisePropertyChangedIfSet(ref _AdvancedMouseRecording, value);
                _conf.AdvancedMouseRecording = value;
            }
        }

        // ファビコンのダウンロード
        private bool _DownloadFavicon = true;
        public bool DownloadFavicon
        {
            get => _DownloadFavicon;
            set
            {
                RaisePropertyChangedIfSet(ref _DownloadFavicon, value);
                _conf.DownloadFavicon = value;
            }
        }


        // MAPS
        public Dictionary<string, Map> Maps = new Dictionary<string, Map>();
        private Map _CurrentMap = new Map();
        public Map CurrentMap
        {
            get => _CurrentMap;
            set { RaisePropertyChangedIfSet(ref _CurrentMap, value); }
        }
        //現在のMAP名
        private string _CurrentMapName = "default";
        public string CurrentMapName
        {
            get => _CurrentMapName;
            set
            {
                if (RaisePropertyChangedIfSet(ref _CurrentMapName, value))
                {
                    IsChangeMap = true;

                    //CurrentMap = Maps[_CurrentMapName];

                }
            }
        }


        // Methods
        // **************************************************
        public void KeyAction(string key)
        {
            if (_conf.Maps[this.CurrentMapName].ContainsKey(key))
            {
                var item = _conf.Maps[this.CurrentMapName][key];
                if (item.ContainsKey("map"))
                {
                    CurrentMapName = item["map"].ToString();
                    return;
                }
                if (item.ContainsKey("path"))
                {
                    var procinfo = new ProcessStartInfo
                    {
                        FileName = item["path"].ToString(),
                        UseShellExecute = true,
                        LoadUserProfile = true,
                        WorkingDirectory = Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH")
                    };
                    if (item.ContainsKey("workingDirectory"))
                    {
                        procinfo.WorkingDirectory = item["workingDirectory"].ToString();
                    }
                    else if (Path.IsPathRooted(item["path"].ToString()))
                    {
                        procinfo.WorkingDirectory = Path.GetDirectoryName(item["path"].ToString());
                    }
                    if (item.ContainsKey("args"))
                    {
                        procinfo.Arguments = item["args"].ToString();
                    }
                    try
                    {
                        Process.Start(procinfo);
                    }
                    catch (Win32Exception e)
                    {
                        Debug.Print(e.Message);
                    }
                    MainWindowVisibility = Visibility.Collapsed;
                }
                if (item.ContainsKey("macro"))
                {
                    MainWindowVisibility = Visibility.Collapsed;
                    Task.Run(() => App.InputMacro.Start(item["macro"].ToString(), int.Parse(item["macrocount"].ToString())));
                }
            }
        }

        public void MapShift(int shift)
        {
            string[] mapArray = Maps.Keys.ToArray();
            for (int i = 0; i < mapArray.Length; i++)
            {
                if (mapArray[i] == CurrentMapName)
                {
                    int index = i + shift;
                    if (index >= mapArray.Length) index = 0;
                    if (index < 0) index = mapArray.Length - 1;
                    CurrentMapName = mapArray[index];
                    return;
                }
            }
        }

        // Configへキーの反映
        public void MapUpdate(object e, Map.MapUpdateEventArgs args)
        {
            var key = args.propertyName;
            Key vmkey = CurrentMap[key];
            if (_conf.Maps[CurrentMapName].ContainsKey(key))
            {
                _conf.Maps[CurrentMapName].Remove(key);
            }
            if (vmkey.Name != null)
            {
                Models.Config.Key confkey = new Models.Config.Key
                {
                    { "name", vmkey.Name }
                };
                if (vmkey.Map != null)
                {
                    confkey.Add("map", vmkey.Map);
                    if (!Maps.ContainsKey(vmkey.Map))
                    {
                        Maps.Add(vmkey.Map, new Map());
                    }
                    if (!_conf.Maps.ContainsKey(vmkey.Map))
                    {
                        _conf.Maps.Add(vmkey.Map, new Models.Config.Map());
                    }

                }
                if (vmkey.Path != null)
                    confkey.Add("path", vmkey.Path);
                if (vmkey.Args != null)
                    confkey.Add("args", vmkey.Args);
                if (vmkey.WorkingDirectory != null)
                    confkey.Add("workingDirectory", vmkey.WorkingDirectory);
                if (vmkey.Icon != null)
                    confkey.Add("icon", vmkey.Icon);
                if (vmkey.Macro != null)
                {
                    confkey.Add("macro", vmkey.Macro);
                    confkey.Add("macrocount", vmkey.MacroCount);
                }
                if (((Color)ColorConverter.ConvertFromString(vmkey.Foreground)).A > 0)
                {
                    confkey.Add("foreground", vmkey.Foreground);
                }
                if (((Color)ColorConverter.ConvertFromString(vmkey.Background)).A > 0)
                {
                    confkey.Add("background", vmkey.Background);
                }

                _conf.Maps[CurrentMapName].Add(key, confkey);
            }
            _conf.Save();
            RaisePropertyChanged("CurrentMap");
        }

        public ViewModel Clone() { return (ViewModel)MemberwiseClone(); }

        // ConfigWindowから変更の反映
        internal void ConfigUpdate(ViewModel vm)
        {
            ShowQwerty = vm.ShowQwerty;
            ShowFunction = vm.ShowFunction;
            ShowNumPad = vm.ShowNumPad;
            DoubleClickSpeed = vm.DoubleClickSpeed;
            AdvancedMouseRecording = vm.AdvancedMouseRecording;
            DownloadFavicon = vm.DownloadFavicon;
        }


        // SubClass
        // **************************************************

        public class Map
        {
            // Properties
            // **************************************************
            public Key F1 { get; set; } = new Key();
            public Key F2 { get; set; } = new Key();
            public Key F3 { get; set; } = new Key();
            public Key F4 { get; set; } = new Key();
            public Key F5 { get; set; } = new Key();
            public Key F6 { get; set; } = new Key();
            public Key F7 { get; set; } = new Key();
            public Key F8 { get; set; } = new Key();
            public Key F9 { get; set; } = new Key();
            public Key F10 { get; set; } = new Key();
            public Key F11 { get; set; } = new Key();
            public Key F12 { get; set; } = new Key();

            public Key D1 { get; set; } = new Key();
            public Key D2 { get; set; } = new Key();
            public Key D3 { get; set; } = new Key();
            public Key D4 { get; set; } = new Key();
            public Key D5 { get; set; } = new Key();
            public Key D6 { get; set; } = new Key();
            public Key D7 { get; set; } = new Key();
            public Key D8 { get; set; } = new Key();
            public Key D9 { get; set; } = new Key();
            public Key D0 { get; set; } = new Key();
            public Key OemMinus { get; set; } = new Key();
            public Key Oem7 { get; set; } = new Key();
            public Key Oem5 { get; set; } = new Key();
            public Key Back { get; set; } = new Key();

            public Key Q { get; set; } = new Key();
            public Key W { get; set; } = new Key();
            public Key E { get; set; } = new Key();
            public Key R { get; set; } = new Key();
            public Key T { get; set; } = new Key();
            public Key Y { get; set; } = new Key();
            public Key U { get; set; } = new Key();
            public Key I { get; set; } = new Key();
            public Key O { get; set; } = new Key();
            public Key P { get; set; } = new Key();
            public Key Oemtilde { get; set; } = new Key();
            public Key OemOpenBrackets { get; set; } = new Key();

            public Key A { get; set; } = new Key();
            public Key S { get; set; } = new Key();
            public Key D { get; set; } = new Key();
            public Key F { get; set; } = new Key();
            public Key G { get; set; } = new Key();
            public Key H { get; set; } = new Key();
            public Key J { get; set; } = new Key();
            public Key K { get; set; } = new Key();
            public Key L { get; set; } = new Key();
            public Key Oemplus { get; set; } = new Key();
            public Key Oem1 { get; set; } = new Key();
            public Key Oem6 { get; set; } = new Key();

            public Key Z { get; set; } = new Key();
            public Key X { get; set; } = new Key();
            public Key C { get; set; } = new Key();
            public Key V { get; set; } = new Key();
            public Key B { get; set; } = new Key();
            public Key N { get; set; } = new Key();
            public Key M { get; set; } = new Key();
            public Key Oemcomma { get; set; } = new Key();
            public Key OemPeriod { get; set; } = new Key();
            public Key OemQuestion { get; set; } = new Key();
            public Key OemBackslash { get; set; } = new Key();

            public Key NumPad0 { get; set; } = new Key();
            public Key NumPad1 { get; set; } = new Key();
            public Key NumPad2 { get; set; } = new Key();
            public Key NumPad3 { get; set; } = new Key();
            public Key NumPad4 { get; set; } = new Key();
            public Key NumPad5 { get; set; } = new Key();
            public Key NumPad6 { get; set; } = new Key();
            public Key NumPad7 { get; set; } = new Key();
            public Key NumPad8 { get; set; } = new Key();
            public Key NumPad9 { get; set; } = new Key();
            public Key Decimal { get; set; } = new Key();

            // Indexer
            // **************************************************
            public Key this[string propertyName]
            {
                get
                {
                    return (Key)typeof(Map).GetProperty(propertyName).GetValue(this);
                }
                set
                {
                    typeof(Map).GetProperty(propertyName).SetValue(this, value);
                    MapUpdateEventHandler(this, new MapUpdateEventArgs()
                    {
                        propertyName = propertyName
                    });
                }
            }

            // Event
            // **************************************************
            internal event EventHandler<MapUpdateEventArgs> MapUpdateEventHandler;

            // SubClass
            // **************************************************
            public class MapUpdateEventArgs
            {
                public string propertyName;
            }

        }

        public class Key : ViewModelBase
        {
            // Constructor
            // **************************************************
            public Key() { }

            public Key(Dictionary<string, object> item)
            {
                _Name = item["name"].ToString();
                if (item.ContainsKey("path")) { _Path = item["path"].ToString(); }
                if (item.ContainsKey("args")) { _Args = item["args"].ToString(); }
                if (item.ContainsKey("workingDirectory")) { _WorkingDirectory = item["workingDirectory"].ToString(); }
                if (item.ContainsKey("map")) { _Map = item["map"].ToString(); }
                if (item.ContainsKey("icon")) { _Icon = item["icon"].ToString(); }
                if (item.ContainsKey("macro")) { _Macro = item["macro"].ToString(); }
                if (item.ContainsKey("macrocount")) { _MacroCount = int.Parse(item["macrocount"].ToString()); }
                if (item.ContainsKey("foreground")) { _Foreground = item["foreground"].ToString(); }
                if (item.ContainsKey("background")) { _Background = item["background"].ToString(); }
                _ = GetImageSource();
            }

            // Properties
            // **************************************************
            private string _Name;
            public string Name
            {
                get => _Name;
                set { RaisePropertyChangedIfSet(ref _Name, value); }
            }
            private string _Path;
            public string Path
            {
                get => _Path;
                set {
                    if (RaisePropertyChangedIfSet(ref _Path, value)) {
                        if(Icon == null && _Path != null)
                        {
                            _ = GetImageSource();
                        }
                    }
                }
            }
            private string _Args;
            public string Args
            {
                get => _Args;
                set { RaisePropertyChangedIfSet(ref _Args, value); }
            }
            private string _WorkingDirectory;
            public string WorkingDirectory
            {
                get => _WorkingDirectory;
                set { RaisePropertyChangedIfSet(ref _WorkingDirectory, value); }
            }
            private string _Map;
            public string Map
            {
                get => _Map;
                set {
                    if(RaisePropertyChangedIfSet(ref _Map, value))
                    {
                        if (value != null && Icon == null)
                        {
                            _ = GetImageSource();
                        }
                    }
                }
            }
            private string _Icon;
            public string Icon
            {
                get => _Icon;
                set {
                    if (string.IsNullOrWhiteSpace(value)) value = null;
                    if (RaisePropertyChangedIfSet(ref _Icon, value)) {
                        _ = GetImageSource();
                    }
                }
            }

            private string _Macro;
            public string Macro
            {
                get => _Macro;
                set
                {
                    if (RaisePropertyChangedIfSet(ref _Macro, value))
                    {
                        if (value != null && Icon == null)
                        {
                            _ = GetImageSource();
                        }
                    }
                }
            }
            private int _MacroCount = 1;
            public int MacroCount
            {
                get => _MacroCount;
                set { RaisePropertyChangedIfSet(ref _MacroCount, value); }
            }


            private System.Windows.Media.ImageSource _Image;
            public System.Windows.Media.ImageSource Image
            {
                get => _Image;
                set { RaisePropertyChangedIfSet(ref _Image, value);}
            }

            private string _Foreground = "#00000000";
            public string Foreground
            {
                get => _Foreground;
                set {
                    if (string.IsNullOrWhiteSpace(value)) value = null; 
                    RaisePropertyChangedIfSet(ref _Foreground, value); 
                }
            }
            private string _Background = "#00000000";
            public string Background
            {
                get => _Background;
                set
                {
                    if (string.IsNullOrWhiteSpace(value)) value = null;
                    RaisePropertyChangedIfSet(ref _Background, value);
                }
            }

            // Methods
            // **************************************************
            public Key Clone() { return (Key)MemberwiseClone(); }

            private async Task GetImageSource()
            {

                if ( Icon != null)
                {
                    if (Regex.IsMatch(Icon, @",[0-9]+$"))
                    {
                        Image = GetImageFromIcon(Icon);

                    }
                    else
                    {
                        Image = new BitmapImage(new Uri(Icon, UriKind.RelativeOrAbsolute));
                    };
                    return;
                }
                if (Map != null)
                {
                    Image = GetImageFromIcon(@"shell32.dll,43");
                    return;
                }
                if (Macro != null)
                {
                    Image = GetImageFromIcon(@"shell32.dll,80");
                    return;
                }

                string ext = System.IO.Path.GetExtension(Path);

                if (Regex.IsMatch(Path, @"^(http|https)://.*"))
                {
                    ext = ".url";
                    if (_conf.DownloadFavicon)
                    {
                        try
                        {
                            var client = new HttpClient();
                            string html = await client.GetStringAsync(Path);
                            string href = "";
                            string[] lines = html.Split('\n');
                            foreach (string line in lines)
                            {
                                if (line.Length < 20000)
                                {
                                    if (line.IndexOf("icon", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        href = Regex.Replace(line, @".*?(rel|REL)=""[^""]*(icon|ICON)[^""]*""[^>]*(href|HREF)=""(.*?)"".*", "$4");
                                        if (href != line)
                                        {
                                            break;
                                        }
                                    }
                                }
                                href = null;
                                if (line.IndexOf("</head>", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    href = Regex.Replace(Path, @"(.*://.*?)/.*", "$1") + "/favicon.ico";
                                    break;
                                }
                            }
                            if (href.Substring(0, 1) == "/") href = Regex.Replace(Path, @"(.*://.*?)/.*", "$1") + href;
                            else if (href.Substring(0, 1) == ".") href = Regex.Replace(Path, @"(.*://.*/).*", "$1") + href;

                            Debug.Print(href);
                            byte[] bytes = await client.GetByteArrayAsync(href);

                            var stream = new MemoryStream(bytes);
                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = stream;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();
                            Image = bitmapImage;
                            return;
                        }
                        catch
                        {
                            Debug.Print("Favicon download Failed.");
                        }
                    }
                }

                if (ext != "") {
                    RegistryKey extKey = Registry.ClassesRoot.OpenSubKey(ext);
                    if (extKey != null)
                    {
                        string cls = (string)extKey.GetValue("");
                        extKey.Close();
                        RegistryKey clsKey = Registry.ClassesRoot.OpenSubKey(cls + @"\DefaultIcon");
                        if (clsKey != null)
                        {
                            string defaultIcon = (string)clsKey.GetValue("");
                            clsKey.Close();
                            if (defaultIcon != @"%1")
                            {
                                string[] values = defaultIcon.Split(',');
                                if (values[0] != "")
                                {
                                    Image = GetImageFromIcon(defaultIcon);
                                    return;
                                }
                            }
                        }
                    }
                }

                // 拡張子が無い場合はフォルダとみなす、チェックすると到達できない場合に固まるから
                if (Path.Substring(0, 2) == @"\\")
                {
                    Image = GetImageFromIcon(@"shell32.dll,275");
                    return;
                }

                if (Directory.Exists(Path))
                {
                    Image = GetImageFromIcon(@"shell32.dll,4");
                    return;
                }
                Image = GetImageFromIcon(Path);
                return;
            }


            private System.Windows.Media.ImageSource GetImageFromIcon(string iconstr, int index = 0) {
                string[] values = iconstr.Split(',');
                string iconPath = values[0];
                if (values.Length == 2)
                {
                    index = int.Parse(values[1]);
                }
                // extract the icon
                System.Windows.Media.ImageSource imgsrc;
                var largeIcons = new IntPtr[1];
                var smallIcons = new IntPtr[1];

                ExtractIconEx(iconPath, index, largeIcons, smallIcons, 1);
                if (largeIcons[0] != IntPtr.Zero)
                {
                    // convert icon handle to ImageSource
                    imgsrc = Imaging.CreateBitmapSourceFromHIcon(largeIcons[0],
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                } else
                {
                    DestroyIcon(largeIcons[0]);
                    DestroyIcon(smallIcons[0]);
                    ExtractIconEx(@"C:\Windows\System32\shell32.dll", 0, largeIcons, smallIcons, 1);
                    imgsrc = Imaging.CreateBitmapSourceFromHIcon(largeIcons[0],
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }

                // clean up
                DestroyIcon(largeIcons[0]);
                DestroyIcon(smallIcons[0]);

                return imgsrc;
            }

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            private static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern bool DestroyIcon(IntPtr handle);
        }

        // テーマ
        private string _Theme;
        public string Theme
        {
            get => _Theme;
            set
            {
                if (RaisePropertyChangedIfSet(ref _Theme, value))
                {
                    _conf.Theme = value;
                    SetThemeColor();
                    OnChangeColor();
                }
            }
        }
        private void SetThemeColor()
        {
            if (Theme == "light" || Theme == "auto" && IsAppsUseLightTheme)
            {
                _Foreground = "#FF000000";
                _Background = "#FFf2f2f9";
                _AccentInfo = "#FF1AA9B3";
                _AccentWarning = "#FFFFFF00";
                _AccentError = "#FFFF0000";

            }
            if (Theme == "dark" || Theme == "auto" && !IsAppsUseLightTheme)
            {
                _Foreground = "#FFFFFFFF";
                _Background = "#FF202020";
                _AccentInfo = "#FF1AA9B3";
                _AccentWarning = "#FFFFFF00";
                _AccentError = "#FFFF0000";
            }
            if (Theme == "custom")
            {
                if (_conf.CustomTheme.ContainsKey("Foreground")) _Foreground = _conf.CustomTheme["Foreground"];
                if (_conf.CustomTheme.ContainsKey("Background")) _Background = _conf.CustomTheme["Background"];
                if (_conf.CustomTheme.ContainsKey("AccentInfo")) _AccentInfo = _conf.CustomTheme["AccentInfo"];
                if (_conf.CustomTheme.ContainsKey("AccentWarning")) _AccentWarning = _conf.CustomTheme["AccentWarning"];
                if (_conf.CustomTheme.ContainsKey("AccentError")) _AccentError = _conf.CustomTheme["AccentError"];
                if (_conf.CustomTheme.ContainsKey("IconColor")) _IconColor = _conf.CustomTheme["IconColor"];

            }
            SetColorResource("Theme.Foreground", Foreground);
            SetColorResource("Theme.Background", Background);
            SetColorResource("Theme.Border", MergeColor(Foreground, Background, 14));
            SetColorResource("Theme.Content.Background", AlphaColor(Foreground, 0x0D));
            SetColorResource("Theme.Content.MouseOver", AlphaColor(Background, 0x10));
            SetColorResource("Theme.Content.Pressed", AlphaColor(Background, 0x0E));
            SetColorResource("Theme.Content.Disabled", AlphaColor(Foreground, 0x23));
            SetColorResource("Theme.Control.Static.Foreground", Foreground);
            SetColorResource("Theme.Control.Static.Background", AlphaColor(Foreground, 0x10));
            SetColorResource("Theme.Control.Static.Border", AlphaColor(Foreground, 0x0E));
            SetColorResource("Theme.Control.Static.Fill", AlphaColor(Foreground, 0xC3));
            SetColorResource("Theme.Control.MouseOver.Background", AlphaColor(Foreground, 0x16));
            SetColorResource("Theme.Control.MouseOver.Border", AccentInfo);
            SetColorResource("Theme.Control.Pressed.Background", AlphaColor(Foreground, 0x0E));
            SetColorResource("Theme.Control.Pressed.Border", AccentInfo);
            SetColorResource("Theme.Control.Focus.Background", AlphaColor(Foreground, 0x0E));
            SetColorResource("Theme.Control.Focus.Border", AccentInfo);
            SetColorResource("Theme.Control.Disabled.Background", AlphaColor(Foreground, 0x02));
            SetColorResource("Theme.Control.Disabled.Border", AlphaColor(Foreground, 0x04));
            SetColorResource("Theme.Control.Popup.Background", MergeColor(Foreground, Background, 5));
            SetColorResource("Theme.Control.Warning.Background", AccentWarning);
            SetColorResource("Theme.Control.Error.Background", AccentError);

            if (Theme == "custom" && _conf.CustomTheme.ContainsKey("CornerRadius"))
            {
                _CornerRadius = _conf.CustomTheme["CornerRadius"];
                App.Current.Resources["Theme.Main.CornerRadius"] = new CornerRadius(double.Parse(_CornerRadius));
            } else
            {
                App.Current.Resources["Theme.Main.CornerRadius"] = new CornerRadius(5);
            }
        }

        // いくつかの色を指定することで中間色を提供する
        private string _Foreground = "#FFFFFFFF";
        public string Foreground
        {
            get => _Foreground;
            set 
            {
                if (IsColorString(value))
                {
                    if (_Theme == "custom")
                    {
                        _conf.CustomTheme["Foreground"] = value;
                        _conf.Save();
                    }
                    if (RaisePropertyChangedIfSet(ref _Foreground, value)) OnChangeColor();
                }
            }
        }
        private string _Background = "#FF202020";
        public string Background
        {
            get => _Background;
            set
            {
                if (IsColorString(value))
                {
                    if (_Theme == "custom")
                    {
                        _conf.CustomTheme["Background"] = value;
                        _conf.Save();
                    }
                    if (RaisePropertyChangedIfSet(ref _Background, value)) OnChangeColor();
                }
            }
        }
        private string _AccentInfo = "#FF1AA9B3";
        public string AccentInfo
        {
            get => _AccentInfo;
            set
            {
                if (IsColorString(value))
                {
                    if (_Theme == "custom")
                    {
                        _conf.CustomTheme["AccentInfo"] = value;
                        _conf.Save();
                    }
                    if (RaisePropertyChangedIfSet(ref _AccentInfo, value)) OnChangeColor();
                }
            }
        }
        private string _AccentWarning = "#FFFFFF00";
        public string AccentWarning
        {
            get => _AccentWarning;
            set 
            {
                if (IsColorString(value))
                {
                    if (_Theme == "custom")
                    {
                        _conf.CustomTheme["AccentWarning"] = value;
                        _conf.Save();
                    }
                    if (RaisePropertyChangedIfSet(ref _AccentWarning, value)) OnChangeColor();
                }
            }
        }
        private string _AccentError = "#FFFF0000";
        public string AccentError
        {
            get => _AccentError;
            set
            {
                if (IsColorString(value))
                {
                    if (_Theme == "custom")
                    {
                        _conf.CustomTheme["AccentError"] = value;
                        _conf.Save();
                    }

                    if (RaisePropertyChangedIfSet(ref _AccentError, value)) OnChangeColor();
                }
            }
        }


        private string _CornerRadius = "5";
        public string CornerRadius
        {
            get => _CornerRadius;
            set
            {
                if(double.TryParse(value, out _))
                {
                       
                    if (_Theme == "custom")
                    {
                        _conf.CustomTheme["CornerRadius"] = value;
                        _conf.Save();
                    }
                    if (RaisePropertyChangedIfSet(ref _CornerRadius, value)) OnChangeColor();
                }
            }
        }
        private string _IconColor = "light";
        public string IconColor
        {
            get => _IconColor;
            set
            {
                if (RaisePropertyChangedIfSet(ref _IconColor, value))
                {
                    if (_Theme == "custom")
                    {
                        _conf.CustomTheme["IconColor"] = value;
                        _conf.Save();
                    }
                    App.ChangeTheme();
                }
            }
        }

        internal void OnChangeColor()
        {
            SetThemeColor();
            App.ChangeTheme();
        }

        private void SetColorResource(string key, string color)
        {
            App.Current.Resources[key] = (Color)ColorConverter.ConvertFromString(color);
        }
        public bool IsColorString(string colorString)
        {
            try
            {
                Color c = (Color)ColorConverter.ConvertFromString(colorString);
                return true;
            }
            catch { return false; }
        }
        public string AlphaColor(string color, byte alpha)
        {
            Color c = (Color)ColorConverter.ConvertFromString(color);
            c.A = alpha;
            return c.ToString().ToUpper();
        }
        public string MergeColor(string color1, string color2, int ratio)
        {
            Color c1 = (Color)ColorConverter.ConvertFromString(color1);
            Color c2 = (Color)ColorConverter.ConvertFromString(color2);
            int alpha = ((c1.A - c2.A) / 100 * ratio) + c2.A;
            int red = ((c1.R - c2.R) / 100 * ratio) + c2.R;
            int green = ((c1.G - c2.G) / 100 * ratio) + c2.G;
            int blue = ((c1.B - c2.B) / 100 * ratio) + c2.B;

            return Color.FromArgb((byte)alpha, (byte)red, (byte)green, (byte)blue).ToString().ToUpper();
        }

        public static bool IsAppsUseLightTheme
        {
            get
            {
                bool nResult;
                string sKeyName = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
                string sSubkeyName = "AppsUseLightTheme";
                Microsoft.Win32.RegistryKey rKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(sKeyName);
                nResult = Convert.ToBoolean(rKey.GetValue(sSubkeyName));
                rKey.Close();
                return nResult;
            }
        }

    }
}