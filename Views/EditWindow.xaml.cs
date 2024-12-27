using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Input;
using System.Text;
using Microsoft.Win32;

namespace QwertyLauncher.Views
{
    /// <summary>
    /// EditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditWindow : Window
    {
        private readonly ViewModel _vm;
        private readonly string _keyName;
        private readonly Key _key;

        internal string Macro
        {
            get => _key.Macro;
            set { _key.Macro = value; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal EditWindow(ViewModel vm, string keyName)
        {
            _vm = vm;
            App.State = "editDialog";
            _keyName = keyName;
            _key = _vm.CurrentMap[keyName].Clone();
            DataContext = _key;
            InitializeComponent();
            AdvancedMouseRecordingToggle.IsChecked = _vm.AdvancedMouseRecording;

            if (_key.Path != null) typeOpen.IsSelected = true;
            if (_key.Map != null) typeMap.IsSelected = true;
            if (_key.Macro != null) typeMacro.IsSelected = true;
            if (_key.PasteStrings != null)
            {
                typePaste.IsSelected = true;
                if (_key.PasteMethod == "Ctrl_V") methodCtrlV.IsSelected = true;
                if (_key.PasteMethod == "Ctrl_Shift_V") methodCtrlShiftV.IsSelected = true;
                if (_key.PasteMethod == "Shift_Insert") methodShiftInsert.IsSelected = true;
            }
            if (_key.Function != null) {
                typeFunction.IsSelected = true;
                if (_key.Function == "OpenConfigDialog") functionOpenConfigDialog.IsSelected = true;
            }
            if (type.SelectedIndex == -1) typeOpen.IsSelected = true;
            map.ItemsSource = _vm.Maps.Keys;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _vm.MainWindowVisibility = Visibility.Visible;
        }

        /// <summary>
        /// 新規MAPの作成
        /// </summary>
        private void NewMap_Click(object sender, RoutedEventArgs e)
        {
            InputBox dlg = new InputBox(App.Current.Resources["String.AddMap"].ToString());
            dlg.ShowDialog();
            if (!string.IsNullOrWhiteSpace(dlg.value.Text))
            {
                string mapname = dlg.value.Text;
                if (!_vm.Maps.ContainsKey(mapname))
                {
                    _vm.Maps.Add(mapname, new Map(_vm));
                }
                map.ItemsSource = _vm.Maps.Keys;
                map.SelectedItem = mapname;
            }
        }

        /// <summary>
        /// ファイルパスのダイヤログ
        /// </summary>
        private void ChoicePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                _key.Path = dialog.FileName;
                if (_key.Name == null)
                {
                    _key.Name = Path.GetFileNameWithoutExtension(dialog.FileName);
                }
            }
        }

        /// <summary>
        /// アイコン（画像ファイル）のダイヤログ
        /// </summary>
        private void ChoiceImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files(*.png; *.jpg; *.jpeg; *.tif; *.tiff; *.bmp; *.gif; *.ico)| *.png; *.jpg; *.jpeg; *.tif; *.tiff; *.bmp; *.gif; *.ico"
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                _key.Icon = dialog.FileName;
            }
        }

        /// <summary>
        /// OKボタン　バリデーション
        /// </summary>
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(name.Text))
            {
                if (!string.IsNullOrWhiteSpace(_key.Path) ||
                    !string.IsNullOrWhiteSpace(_key.Map) ||
                    !string.IsNullOrWhiteSpace(_key.Macro) ||
                    !string.IsNullOrWhiteSpace(_key.PasteStrings)||
                    !string.IsNullOrWhiteSpace(_key.Function)
                    )
                {
                    _vm.CurrentMap[_keyName] = _key.Clone();
                    Close();
                }
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            _vm.CurrentMap[_keyName] = new Key(_vm);
            Close();
        }


        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!typeOpen.IsSelected)
            {
                _key.Path = null;
                _key.Args = null;
                _key.WorkingDirectory = null;
            }
            if (!typeMap.IsSelected)
            {
                _key.Map = null;
            }
            if (!typeMacro.IsSelected)
            {
                _key.Macro = null;
                _key.MacroCount = 1;
            }
            
            if (typePaste.IsSelected)
            {
                if (pastemethod.SelectedIndex == -1) pastemethod.SelectedIndex = 0;
            }
            else
            {
                _key.PasteStrings = null;
                _key.PasteMethod = null;
            }
            if (typeFunction.IsSelected)
            {
                if (function.SelectedIndex == -1) { pastemethod.SelectedIndex = 0; }
            }
            else
            {
                _key.Function = null;
            }
            Debug.Print("type_SelectionChanged");

        }

        private void PasteMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (methodCtrlV.IsSelected) _key.PasteMethod = "Ctrl_V";
            else if (methodCtrlShiftV.IsSelected) _key.PasteMethod = "Ctrl_Shift_V";
            else  _key.PasteMethod = "Shift_Insert";
            

            Debug.Print("PasteMethod_SelectionChanged");

        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int PickIconDlg(IntPtr hwndOwner, StringBuilder lpstrFile, int nMaxFile, ref int lpdwIconIndex);
        private const int MAX_PATH = 0x00000104;

        private void ChoiceIcon_Click(object sender, RoutedEventArgs e)
        {
            // show the Pick Icon Dialog
            int index = 0;
            int retval;
            var handle = new WindowInteropHelper(this).Handle;
            var iconfile = "imageres.dll";
            var sb = new StringBuilder(iconfile, MAX_PATH);
            retval = PickIconDlg(handle, sb, sb.MaxCapacity, ref index);
            if (retval != 0)
            {
                _key.Icon = sb.ToString() + "," + index.ToString();
            }
        }
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out int _);
        }

        private void DoubleOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".") ((TextBox)sender).Text = "0.1";

            e.Handled = !double.TryParse(e.Text, out double _);
        }

        private void MacroSpeedUp_Click(object sender, RoutedEventArgs e)
        {
            _key.Macro = ChangeMacroSpeed(_key.Macro, 2);
        }
        private void MacroSpeedDown_Click(object sender, RoutedEventArgs e)
        {
            _key.Macro = ChangeMacroSpeed(_key.Macro, 0.5);
        }
        private string ChangeMacroSpeed(string macro, double rate)
        {
            string newmacro = "";
            string[] lines = macro.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) break;
                string[] seq = line.Split(',');
                if (long.TryParse(seq[0], out long ms))
                {
                    seq[0] = Math.Floor((ms / rate)).ToString();
                }
                string newline = string.Join(",", seq);
                newmacro += newline + "\r\n";
            }
            return newmacro;
        }

        private void MacroRecord_Click(object sender, RoutedEventArgs e)
        {
            App.StartMacroRecord();
        }

        
        
        private void AdvancedMouseRecording_Change(object sender, RoutedEventArgs e)
        {
            _vm.AdvancedMouseRecording = (bool)AdvancedMouseRecordingToggle.IsChecked;
        }

        private void Function_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (functionOpenConfigDialog.IsSelected == true)
            {
                _key.Function = "OpenConfigDialog";
            }
        }
    }
}