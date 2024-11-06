using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            _ActivateKeys = _conf.ActivateKeys;
            _ActivateWithTaskbarDoubleClick = _conf.ActivateWithTaskbarDoubleClick;
            _ShowQwerty = _conf.ShowQwerty;
            _ShowFunction = _conf.ShowFunction;
            _ShowNumPad = _conf.ShowNumPad;
            _DoubleClickSpeed = _conf.DoubleClickSpeed;
            _AdvancedMouseRecording = _conf.AdvancedMouseRecording;

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
                if (value == _MainWindowVisibility)
                    return;
                if (value == Visibility.Visible)
                {
                    IsActive = true;
                    Screen s = App.GetCurrentScreen();

                    int width = Math.Max(s.Bounds.Width, s.Bounds.Height);
                    int height = Math.Min(s.Bounds.Width, s.Bounds.Height) / 2;
                    width = width / 16 * 9;

                    MainWindowLeft = s.Bounds.Left + ((s.Bounds.Width - width) / 2);
                    MainWindowTop = s.Bounds.Top + s.WorkingArea.Height - height;
                    MainWindowWidth = width;
                    MainWindowHeight = height;
                }
                else if (value == Visibility.Collapsed)
                {
                    IsActive = false;
                }
                _MainWindowVisibility = value;
                RaisePropertyChanged();
            }
        }



        private readonly Models.Config _conf;

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


        // テーマ
        private string _Theme;
        public string Theme
        {
            get => _Theme;
            set
            {
                RaisePropertyChangedIfSet(ref _Theme, value);
                _conf.Theme = value;
            }
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
                if (item.ContainsKey("path")) { Path = item["path"].ToString(); }
                if (item.ContainsKey("args")) { Args = item["args"].ToString(); }
                if (item.ContainsKey("workingDirectory")) { WorkingDirectory = item["workingDirectory"].ToString(); }
                if (item.ContainsKey("map")) { Map = item["map"].ToString(); }
                if (item.ContainsKey("icon")) { Icon = item["icon"].ToString(); }
                if (item.ContainsKey("macro")) { Macro = item["macro"].ToString(); }
                if (item.ContainsKey("macrocount")) { MacroCount = int.Parse(item["macrocount"].ToString()); }
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
                            MakeImage();
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
                            MakeImage();
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
                        if(value != null)
                        {
                            MakeImage();
                        }
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
                            MakeImage();
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


            private ImageSource _Image;
            public ImageSource Image
            {
                get => _Image;
                set { RaisePropertyChangedIfSet(ref _Image, value);}
            }

            // Methods
            // **************************************************
            public Key Clone() { return (Key)MemberwiseClone(); }

            private void MakeImage()
            {

                if ( Icon != null)
                {
                    Image = GetImage(Icon);
                    return;
                }
                if (Map != null)
                {
                    Image = GetImage(@"shell32.dll,43");
                    return;
                }
                if (Macro != null)
                {
                    Image = GetImage(@"shell32.dll,80");
                    return;
                }

                if (Directory.Exists(Path))
                {
                    if (File.GetAttributes(Path).HasFlag(FileAttributes.Directory))
                    {
                        if (Path.Substring(0, 2) == @"\\")
                        {
                            Image = GetImage(@"shell32.dll,275");
                        }
                        else
                        {
                            Image = GetImage(@"shell32.dll,4");
                        }
                        return;
                    }
                }

                string ext;
                if (Regex.IsMatch(Path, @"^(http|https)://.*"))
                {
                    ext = ".url";
                }
                else
                {
                    ext = System.IO.Path.GetExtension(Path);
                }
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
                                Image = GetImage(defaultIcon);
                                return;
                            }
                        }
                    }
                }
                Image = GetImage(Path);
                return;
            }

            private ImageSource GetImage(string iconstr, int index = 0) {
                string[] values = iconstr.Split(',');
                string iconPath = values[0];
                if (values.Length == 2)
                {
                    index = int.Parse(values[1]);
                }
                // extract the icon
                ImageSource imgsrc;
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



    }
}