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

        internal EditWindow(ViewModel vm, string keyName)
        {
            _vm = vm;
            _vm.IsDialogOpen = true;
            _keyName = keyName;
            _key = _vm.CurrentMap[keyName].Clone();
            DataContext = _key;
            InitializeComponent();
            AdvancedMouseRecordingToggle.IsChecked = _vm.AdvancedMouseRecording;

            if (_key.Path != null) typeOpen.IsSelected = true;
            if (_key.Map != null) typeMap.IsSelected = true;
            if (_key.Macro != null) typeMacro.IsSelected = true;
            if (type.SelectedIndex == -1) typeOpen.IsSelected = true;
            map.ItemsSource = _vm.Maps.Keys;
        }

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

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(name.Text))
            {
                if (!string.IsNullOrWhiteSpace(path.Text) ||
                    !string.IsNullOrWhiteSpace(map.Text) ||
                    !string.IsNullOrWhiteSpace(macro.Text))
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
            Debug.Print("type_SelectionChanged");

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

        private void OnClosed(object sender, EventArgs e)
        {
            _vm.IsDialogOpen = false;
            _vm.MainWindowVisibility = Visibility.Visible;
        }

        private void AdvancedMouseRecording_Change(object sender, RoutedEventArgs e)
        {
            _vm.AdvancedMouseRecording = (bool)AdvancedMouseRecordingToggle.IsChecked;
        }
    }
}