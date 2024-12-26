using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using static QwertyLauncher.Views.InputHook;

namespace QwertyLauncher.Views
{
    /// <summary>
    /// ConfigWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigWindow : Window
    {
        internal ConfigWindow(ViewModel vm)
        {
            _vm = vm;
            App.State = "configDialog";

            InitializeComponent();

            if (_vm.Theme == "auto") themeAuto.IsSelected = true;
            if (_vm.Theme == "light") themeLight.IsSelected = true;
            if (_vm.Theme == "dark") themeDark.IsSelected = true;
            if (_vm.Theme == "custom") themeCustom.IsSelected = true;
            if (_vm.IconColor == "light") iconLight.IsSelected = true;
            if (_vm.IconColor == "dark") iconDark.IsSelected = true;

            Title = App.Name;
            DataContext = _vm;

            _CanThemeChange = true;
        }
        private ViewModel _vm;
        private bool _CanThemeChange = false;

        private void Window_Closed(object sender, EventArgs e)
        {
            App.State = "ready";
        }

        private void DoubleClickSpeed_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out int _);
        }

        private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_CanThemeChange)
            {
                if (themeAuto.IsSelected) _vm.Theme = "auto";
                if (themeLight.IsSelected) _vm.Theme = "light";
                if (themeDark.IsSelected) _vm.Theme = "dark";
                if (themeCustom.IsSelected) _vm.Theme = "custom";
            }
        }
        private void IconColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_CanThemeChange)
            {
                if (iconLight.IsSelected) _vm.IconColor = "light";
                if (iconDark.IsSelected) _vm.IconColor = "dark";
            }
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
        private void MaximizeOrRestoreWindow(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
                SystemCommands.MaximizeWindow(this);
            else
                SystemCommands.RestoreWindow(this);
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }


        private void InputHook_OnKeyboardHookEvent(object sender, KeyboardHookEventArgs e)
        {
            if (e.Msg == "KEYDOWN") {
                if (_vm.ActivateKeys.Contains(e.Key.ToString()))
                {
                    var list = new List<string>();
                    list.AddRange(_vm.ActivateKeys);
                    list.Remove(e.Key.ToString());
                    string[] newActivateKeys;
                    newActivateKeys = list.ToArray();
                    _vm.ActivateKeys = newActivateKeys;
                } else
                {
                    string[] newActivateKeys = _vm.ActivateKeys;
                    Array.Resize(ref newActivateKeys, newActivateKeys.Length + 1);
                    newActivateKeys[newActivateKeys.Length - 1] = e.Key.ToString();
                    _vm.ActivateKeys = newActivateKeys;
                }
            }
        }

        private void ActivateKeys_GotFocus(object sender, RoutedEventArgs e)
        {
            App.InputHook.OnKeyboardHookEvent += InputHook_OnKeyboardHookEvent;
        }

        private void ActivateKeys_LostFocus(object sender, RoutedEventArgs e)
        {
            App.InputHook.OnKeyboardHookEvent -= InputHook_OnKeyboardHookEvent;

        }

    }
}
