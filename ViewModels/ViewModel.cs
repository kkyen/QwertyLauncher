using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace QwertyLauncher
{
    public class ViewModel : ViewModelBase
    {
        public static string Name { get;} = App.Name;
        public static string Version { get;} = App.Version;
        // Constructor
        // **************************************************
        public ViewModel()
        {
            InitializeConfig();
            InitializeTheme();
        }

        private void InitializeConfig()
        {
            _conf = new Models.Config(App.ConfigPath);

            _ActivateKeys = _conf.ActivateKeys;
            _ActivateWithTaskbarDoubleClick = _conf.ActivateWithTaskbarDoubleClick;
            _ModKeys = _conf.ModKeys;
            _SeparateModSides = _conf.SeparateModSides;
            _ShowQwerty = _conf.ShowQwerty;
            _ShowFunction = _conf.ShowFunction;
            _ShowNumPad = _conf.ShowNumPad;
            _DoubleClickSpeed = _conf.DoubleClickSpeed;
            _AdvancedMouseRecording = _conf.AdvancedMouseRecording;
            _DownloadFavicon = _conf.DownloadFavicon;
            _autoUpdate = _conf.AutoUpdate;
            foreach (string mapname in _conf.Maps.Keys)
            {
                Map map = new Map(this);
                map.Name = mapname;
                map.MapUpdateEventHandler += MapUpdate;
                foreach (string modname in _conf.Maps[mapname].Keys)
                {
                    var mod = new Mod(this);
                    mod.Name = modname;
                    mod.ModUpdateEventHandler += map.ModUpdate;

                    foreach (string keyname in _conf.Maps[mapname][modname].Keys)
                    {
                        var item = _conf.Maps[mapname][modname][keyname];
                        Key key = new Key(this, item);
                        mod.GetType().GetProperty(keyname).SetValue(mod, key);
                    }
                    map.Mods.Add(modname, mod);
                }
                Maps.Add(mapname, map);
            }
            _CurrentMap = Maps[CurrentMapName]["default"];
        }

        private void InitializeTheme()
        {
            _Theme = _conf.Theme;
            SetThemeColor();
            if (_conf.CustomTheme.ContainsKey("Foreground")) _Foreground = _conf.CustomTheme["Foreground"];
            if (_conf.CustomTheme.ContainsKey("Background")) _Background = _conf.CustomTheme["Background"];
            if (_conf.CustomTheme.ContainsKey("AccentInfo")) _AccentInfo = _conf.CustomTheme["AccentInfo"];
            if (_conf.CustomTheme.ContainsKey("AccentWarning")) _AccentWarning = _conf.CustomTheme["AccentWarning"];
            if (_conf.CustomTheme.ContainsKey("AccentError")) _AccentError = _conf.CustomTheme["AccentError"];
            if (_conf.CustomTheme.ContainsKey("CornerRadius")) _CornerRadius = _conf.CustomTheme["CornerRadius"];
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
                        App.State = "active";
                        SetMainWindowPosition();
                    }
                    else if (value == Visibility.Collapsed)
                    {
                        App.State = "ready";
                    }
                }
            }
        }

        private void SetMainWindowPosition()
        {
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

        private static Models.Config _conf;

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
        public void AddActivateKeys(string value)
        {
            ActivateKeys = AddStringArray(ActivateKeys, value);
        }
        public void RemoveActivateKeys(string value)
        {
            ActivateKeys = RemoveStringArray(ActivateKeys, value);
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

        // ModKeys
        private string[] _ModKeys;
        public string[] ModKeys
        {
            get => _ModKeys;
            set
            {
                RaisePropertyChangedIfSet(ref _ModKeys, value);
                RaisePropertyChanged("JoinModKeys");
                _conf.ModKeys = value;
            }
        }
        public void AddModKeys(string value)
        {
            ModKeys = AddStringArray(ModKeys, value);
        }
        public void RemoveModKeys(string value)
        {
            ModKeys = RemoveStringArray(ModKeys, value);
        }
        public string JoinModKeys
        {
            get => string.Join(",", _ModKeys);
            set
            {
                var array = value.Split(',');
                RaisePropertyChangedIfSet(ref _ModKeys, array);
                _conf.ModKeys = array;
            }
        }
        // モッドキーを左右に分ける
        private bool _SeparateModSides;
        public bool SeparateModSides
        {
            get => _SeparateModSides;
            set
            {
                RaisePropertyChangedIfSet(ref _SeparateModSides, value);
                _conf.SeparateModSides = value;
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
        private bool _DownloadFavicon;
        public bool DownloadFavicon
        {
            get => _DownloadFavicon;
            set
            {
                RaisePropertyChangedIfSet(ref _DownloadFavicon, value);
                _conf.DownloadFavicon = value;
            }
        }

        /// <summary>
        /// 自動更新
        /// </summary>
        private bool _autoUpdate;
        public bool AutoUpdate
        {
            get => _autoUpdate;
            set
            {
                RaisePropertyChangedIfSet(ref _autoUpdate, value);
                _conf.AutoUpdate = value;
            }
        }
        /// <summary>
        /// 自動起動
        /// </summary>
        public bool AutoStart
        {
            get
            {
                using (var registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    return registryKey.GetValue(Name) != null;
                }
            }
            set
            {
                if (value)
                {
                    using (var registryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                    {
                        registryKey.SetValue(Name, Assembly.GetEntryAssembly().Location);
                    }
                }
                else
                {
                    using (var registryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                    {
                        registryKey.DeleteValue(Name, false);
                    }
                }
            }
        }

        // MAPS
        public Dictionary<string, Map> Maps = new Dictionary<string, Map>();


        private Mod _CurrentMap;
        public Mod CurrentMap
        {
            get => _CurrentMap;
            set { RaisePropertyChangedIfSet(ref _CurrentMap, value); }
        }
        //現在のMAP名
        private string _CurrentMapName = "Root";
        public string CurrentMapName
        {
            get => _CurrentMapName;
            set
            {
                if (RaisePropertyChangedIfSet(ref _CurrentMapName, value))
                {
                    if (!Maps.ContainsKey(value))
                    {
                        var map = new Map(this) { Name = CurrentMapName };
                        var mod = new Mod(this) { Name = CurrentMod};
                        mod.ModUpdateEventHandler += map.ModUpdate;
                        map.MapUpdateEventHandler += MapUpdate;
                        map.Mods.Add(CurrentMod, mod);
                        Maps.Add(CurrentMapName, map);
                    }
                    IsChangeMap = true;

                    //CurrentMap = Maps[_CurrentMapName];

                }
            }
        }
        private string _CurrentMod = "default";
        public string CurrentMod
        {
            get => _CurrentMod;
            set
            {
                if (RaisePropertyChangedIfSet(ref _CurrentMod, value))
                {
                    if (!Maps[CurrentMapName].Mods.ContainsKey(value))
                    {
                        var mod = new Mod(this) { Name = CurrentMod };
                        mod.ModUpdateEventHandler += Maps[CurrentMapName].ModUpdate;
                        Maps[CurrentMapName].Mods.Add(value, mod);
                    }
                    IsChangeMap = true;
                }
            }
        }
        public void AddCurrentMod(string value)
        {
            if (CurrentMod == "default")
            {
                CurrentMod = value;
                return;
            } else
            {
                var array = CurrentMod.Split(',');
                if (!array.Contains(value))
                {
                    CurrentMod = string.Join(",", AddStringArray(array, value));
                }
            }
        }
        public void RemoveCurrentMod(string value)
        {
            var array = RemoveStringArray(CurrentMod.Split(','), value);
            if (array.Length == 0)
            {
                CurrentMod = "default";
            } else
            {
                CurrentMod = string.Join(",", array);
            }
        }

        // Methods
        // **************************************************


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
            var map = args.mapName;
            var mod = args.modName;
            var key = args.keyName;
            Key vmkey = Maps[map][mod][key];
            if (!_conf.Maps.ContainsKey(map))
            {
                _conf.Maps.Add(map, new Models.Config.Map());
            }
            if (!_conf.Maps[map].ContainsKey(mod))
            {
                _conf.Maps[map].Add(mod, new Models.Config.Mod());
            }
            if (_conf.Maps[map][mod].ContainsKey(key))
            {
                _conf.Maps[map][mod].Remove(key);
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
                        Maps.Add(vmkey.Map, new Map(this));
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
                    confkey.Add("macrospeed", vmkey.MacroSpeed);
                }
                if (vmkey.PasteStrings != null){
                    confkey.Add("pasteStrings", vmkey.PasteStrings);
                    confkey.Add("pasteMethod", vmkey.PasteMethod);
                }
                if (vmkey.Function != null)
                {
                    confkey.Add("function", vmkey.Function);
                }
                if (((Color)ColorConverter.ConvertFromString(vmkey.Foreground)).A > 0)
                {
                    confkey.Add("foreground", vmkey.Foreground);
                }
                if (((Color)ColorConverter.ConvertFromString(vmkey.Background)).A > 0)
                {
                    confkey.Add("background", vmkey.Background);
                }

                _conf.Maps[map][mod].Add(key, confkey);
            }

            if (_conf.Maps[map][mod].Keys.Count == 0)
            {
                _conf.Maps[map].Remove(mod);
            }

            if (_conf.Maps[map].Keys.Count == 0)
            {
                _conf.Maps.Remove(map);
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
            AutoUpdate = vm.AutoUpdate;
        }

        // 新規登録用一時キー
        internal Key NewKey { get; set; } = null;


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
            SetColorResource("Theme.Message.Background", AlphaColor(Background, 0x80));

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
        public string BackgroundTransparent
        {
            get => AlphaColor(Background, 0x01);
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

        private string[] RemoveStringArray(string[] array, string value)
        {
            List<string> list = new List<string>(array);
            list.Remove(value);
            return list.ToArray();
        }

        private string[] AddStringArray(string[] array, string value)
        {
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = value;
            List<string> list = new List<string>(array);
            list.Sort();
            return list.ToArray();
        }

    }
}