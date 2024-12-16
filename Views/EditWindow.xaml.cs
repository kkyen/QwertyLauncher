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
        private readonly ViewModel Context;
        private readonly string _key;
        private readonly ViewModel.Key _datacontext;

        internal string Macro
        {
            get => _datacontext.Macro;
            set { _datacontext.Macro = value; }
        }

        internal EditWindow(ViewModel vm, string key)
        {
            Context = vm;
            Context.IsDialogOpen = true;
            _key = key;
            _datacontext = Context.CurrentMap[key].Clone();
            DataContext = _datacontext;
            InitializeComponent();
            RecordingPanel.DataContext = vm;

            if (_datacontext.Path != null) typeOpen.IsSelected = true;
            if (_datacontext.Map != null) typeMap.IsSelected = true;
            if (_datacontext.Macro != null) typeMacro.IsSelected = true;
            if (type.SelectedIndex == -1) typeOpen.IsSelected = true;
            map.ItemsSource = Context.Maps.Keys;
        }

        private void NewMap_Click(object sender, RoutedEventArgs e)
        {
            InputBox dlg = new InputBox(App.Current.Resources["String.AddMap"].ToString());
            dlg.ShowDialog();
            if (!string.IsNullOrWhiteSpace(dlg.value.Text))
            {
                string mapname = dlg.value.Text;
                if (!Context.Maps.ContainsKey(mapname))
                {
                    Context.Maps.Add(mapname, new ViewModel.Map());
                }
                map.ItemsSource = Context.Maps.Keys;
                map.SelectedItem = mapname;
            }
        }

        private void ChoicePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                _datacontext.Path = dialog.FileName;
                if (_datacontext.Name == null)
                {
                    _datacontext.Name = Path.GetFileNameWithoutExtension(dialog.FileName);
                }
            }
        }

        private void ChoiceImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Image Files(*.png; *.jpg; *.jpeg; *.tif; *.tiff; *.bmp; *.gif; *.ico)| *.png; *.jpg; *.jpeg; *.tif; *.tiff; *.bmp; *.gif; *.ico";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                _datacontext.Icon = dialog.FileName;
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
                    Context.CurrentMap[_key] = _datacontext.Clone();
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
            Context.CurrentMap[_key] = new ViewModel.Key();
            Close();
        }


        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!typeOpen.IsSelected)
            {
                _datacontext.Path = null;
                _datacontext.Args = null;
                _datacontext.WorkingDirectory = null;
            }
            if (!typeMap.IsSelected)
            {
                _datacontext.Map = null;
            }
            if (!typeMacro.IsSelected)
            {
                _datacontext.Macro = null;
                _datacontext.MacroCount = 1;
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
                _datacontext.Icon = sb.ToString() + "," + index.ToString();
            }
        }
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out int _);
        }
        private void MacroSpeedUp_Click(object sender, RoutedEventArgs e)
        {
            _datacontext.Macro = ChangeMacroSpeed(_datacontext.Macro, 2);
        }
        private void MacroSpeedDown_Click(object sender, RoutedEventArgs e)
        {
            _datacontext.Macro = ChangeMacroSpeed(_datacontext.Macro, 0.5);
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
            Context.IsDialogOpen = false;
            Context.MainWindowVisibility = Visibility.Visible;
        }
    }
}