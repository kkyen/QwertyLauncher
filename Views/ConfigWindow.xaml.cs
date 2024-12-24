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
            Context = vm;
            Context.IsDialogOpen = true;
            DataContext = Context;
            Title = App.Name;

            InitializeComponent();

            if (Context.Theme == "auto") themeAuto.IsSelected = true;
            if (Context.Theme == "light") themeLight.IsSelected = true;
            if (Context.Theme == "dark") themeDark.IsSelected = true;
            if (Context.Theme == "custom") themeCustom.IsSelected = true;
            if (Context.IconColor == "light") iconLight.IsSelected = true;
            if (Context.IconColor == "dark") iconDark.IsSelected = true;

            _CanThemeChange = true;
        }
        private ViewModel Context;
        private bool _CanThemeChange = false;

        private void Window_Closed(object sender, EventArgs e)
        {
            Context.IsDialogOpen = false;
        }

        private void DoubleClickSpeed_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out int _);
        }

        private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_CanThemeChange)
            {
                if (themeAuto.IsSelected) Context.Theme = "auto";
                if (themeLight.IsSelected) Context.Theme = "light";
                if (themeDark.IsSelected) Context.Theme = "dark";
                if (themeCustom.IsSelected) Context.Theme = "custom";
            }
        }
        private void IconColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_CanThemeChange)
            {
                if (iconLight.IsSelected) Context.IconColor = "light";
                if (iconDark.IsSelected) Context.IconColor = "dark";
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
                if (Context.ActivateKeys.Contains(e.Key.ToString()))
                {
                    var list = new List<string>();
                    list.AddRange(Context.ActivateKeys);
                    list.Remove(e.Key.ToString());
                    string[] newActivateKeys;
                    newActivateKeys = list.ToArray();
                    Context.ActivateKeys = newActivateKeys;
                } else
                {
                    string[] newActivateKeys = Context.ActivateKeys;
                    Array.Resize(ref newActivateKeys, newActivateKeys.Length + 1);
                    newActivateKeys[newActivateKeys.Length - 1] = e.Key.ToString();
                    Context.ActivateKeys = newActivateKeys;
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
