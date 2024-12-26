using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel;
using static QwertyLauncher.Views.InputMacro;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Media;

namespace QwertyLauncher
{
    public class Key : ViewModelBase
    {
        // Constructor
        // **************************************************
        public Key(ViewModel vm) {
            _vm = vm;
        }

        public Key(ViewModel vm, Dictionary<string, object> item)
        {
            _vm = vm;
            _Name = item["name"].ToString();
            if (item.ContainsKey("path")) { _Path = item["path"].ToString(); }
            if (item.ContainsKey("args")) { _Args = item["args"].ToString(); }
            if (item.ContainsKey("workingDirectory")) { _WorkingDirectory = item["workingDirectory"].ToString(); }
            if (item.ContainsKey("map")) { _Map = item["map"].ToString(); }
            if (item.ContainsKey("icon")) { _Icon = item["icon"].ToString(); }
            if (item.ContainsKey("macro")) { _Macro = item["macro"].ToString(); }
            if (item.ContainsKey("macrocount")) { _MacroCount = int.Parse(item["macrocount"].ToString()); }
            if (item.ContainsKey("macrospeed")) { _MacroSpeed = int.Parse(item["macrospeed"].ToString()); }
            if (item.ContainsKey("pasteStrings")) { _PasteStrings = item["pasteStrings"].ToString(); }
            if (item.ContainsKey("pasteMethod")) { _PasteMethod = item["pasteMethod"].ToString(); }
            if (item.ContainsKey("function")) { _function = item["function"].ToString(); }
            if (item.ContainsKey("foreground")) { _Foreground = item["foreground"].ToString(); }
            if (item.ContainsKey("background")) { _Background = item["background"].ToString(); }
            _ = GetImageSource();
        }

        // Properties
        // **************************************************
        private ViewModel _vm;
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
            set
            {
                if (RaisePropertyChangedIfSet(ref _Path, value))
                {
                    if (Icon == null && _Path != null)
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
            set
            {
                if (RaisePropertyChangedIfSet(ref _Map, value))
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
            set
            {
                if (string.IsNullOrWhiteSpace(value)) value = null;
                if (RaisePropertyChangedIfSet(ref _Icon, value))
                {
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

        private double _MacroSpeed = 1;
        public double MacroSpeed
        {
            get => _MacroSpeed;
            set { RaisePropertyChangedIfSet(ref _MacroSpeed, value); }
        }

        private string _PasteStrings;
        public string PasteStrings
        {
            get => _PasteStrings;
            set { RaisePropertyChangedIfSet(ref _PasteStrings, value); }
        }

        private string _PasteMethod;
        public string PasteMethod
        {
            get => _PasteMethod;
            set { RaisePropertyChangedIfSet(ref _PasteMethod, value); }
        }

        private string _function;
        public string Function
        {
            get => _function;
            set { RaisePropertyChangedIfSet(ref _function, value); }
        }

        private System.Windows.Media.ImageSource _Image;
        public System.Windows.Media.ImageSource Image
        {
            get => _Image;
            set { RaisePropertyChangedIfSet(ref _Image, value); }
        }

        private string _Foreground = "#00000000";
        public string Foreground
        {
            get => _Foreground;
            set
            {
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
        public void Action()
        {
            if (_vm.NewKey != null)
            {
                Name = _vm.NewKey.Name;
                Path = _vm.NewKey.Path;
                _vm.NewKey = null;
                return;
            }
            if (Map != null)
            {
                if (_vm.Maps.ContainsKey(Map))
                {
                    _vm.CurrentMapName = Map;
                    return;
                }
                else
                {
                    /// mapが存在しない場合
                }
            }
            if (Path != null)
            {
                var procinfo = new ProcessStartInfo
                {
                    FileName = Path,
                    UseShellExecute = true,
                    LoadUserProfile = true,
                    WorkingDirectory = Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH")
                };
                if (WorkingDirectory != null)
                {
                    procinfo.WorkingDirectory = WorkingDirectory;
                }
                else if (System.IO.Path.IsPathRooted(Path))
                {
                    procinfo.WorkingDirectory = System.IO.Path.GetDirectoryName(Path);
                }
                if (Args != null)
                {
                    procinfo.Arguments = Args;
                }
                try
                {
                    Process.Start(procinfo);
                }
                catch (Win32Exception e)
                {
                    Debug.Print(e.Message);
                }
                _vm.MainWindowVisibility = Visibility.Collapsed;
            }
            if (Macro != null)
            {
                _vm.MainWindowVisibility = Visibility.Collapsed;
                Task.Run(() => App.InputMacro.Start(Macro, MacroCount, MacroSpeed));
                return;
            }
            if (PasteStrings != null)
            {
                _vm.MainWindowVisibility = Visibility.Collapsed;
                System.Threading.Thread t = new System.Threading.Thread(() => {
                    System.Windows.Clipboard.Clear();
                    System.Windows.Clipboard.SetText(PasteStrings); 
                });
                t.SetApartmentState(System.Threading.ApartmentState.STA);
                t.Start();
                t.Join();
                string pastemethod = "";
                if (PasteMethod == "Ctrl_V")
                {
                    pastemethod = "0,KEYBOARD,KEYDOWN,LControlKey\r\n";
                    pastemethod += "0,KEYBOARD,KEYDOWN,V\r\n";
                    pastemethod += "0,KEYBOARD,KEYUP,LControlKey\r\n";
                    pastemethod += "0,KEYBOARD,KEYUP,V";
                }
                if (PasteMethod == "Ctrl_Shift_V")
                {
                    pastemethod = "0,KEYBOARD,KEYDOWN,LControlKey\r\n";
                    pastemethod += "0,KEYBOARD,KEYDOWN,LShiftKey\r\n";
                    pastemethod += "0,KEYBOARD,KEYDOWN,V\r\n";
                    pastemethod += "0,KEYBOARD,KEYUP,LControlKey\r\n";
                    pastemethod += "0,KEYBOARD,KEYUP,LShiftKey\r\n";
                    pastemethod += "0,KEYBOARD,KEYUP,V";
                }
                if (PasteMethod == "Shift_Insert")
                {
                    pastemethod = "0,KEYBOARD,KEYDOWN,LShiftKey\r\n";
                    pastemethod += "0,KEYBOARD,KEYDOWN,Insert\r\n";
                    pastemethod += "0,KEYBOARD,KEYUP,LShiftKey\r\n";
                    pastemethod += "0,KEYBOARD,KEYUP,Insert";
                }
                Task.Run(() => App.InputMacro.Start(pastemethod, 1, 1));
            }
            if(Function != null)
            {
                _vm.MainWindowVisibility = Visibility.Collapsed;
                if (Function == "OpenConfigDialog")
                {
                    App.OpenConfigDialog();
                }
            }
        }

        public Key Clone() { return (Key)MemberwiseClone(); }

        private async Task GetImageSource()
        {

            if (Icon != null)
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
            if (PasteStrings != null)
            {
                Image = GetImageFromIcon(@"imageres.dll,363");
                return;
            }
            if (Function != null)
            {
                /// resourceのfa
                Image = new BitmapImage(new Uri("/Resources/favicon.ico", UriKind.Relative));
            }

            string ext = System.IO.Path.GetExtension(Path);

            if (Regex.IsMatch(Path, @"^(http|https)://.*"))
            {
                ext = ".url";
                if (_vm.DownloadFavicon)
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

            if (ext != "")
            {
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
            if (Path.Substring(0, 2) == @"\\" && ext == "")
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


        private System.Windows.Media.ImageSource GetImageFromIcon(string iconstr, int index = 0)
        {
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
            }
            else
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
