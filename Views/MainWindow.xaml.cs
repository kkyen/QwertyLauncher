using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QwertyLauncher.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel _vm;
        private bool _isMouseDown = false;
        private bool _isKeyAreaFocus = false;
        private Point _point;
        internal EditWindow EditView;
        private string _DragSrcKey;
        private string _DragSrcMap;
        private string _DragSrcMod;
        private DragDropEffects _DragEffect;


        internal MainWindow(ViewModel datacontext)
        {
            InitializeComponent();
            _vm = datacontext;
            DataContext = datacontext;
            KeyArea.AddHandler(Button.ClickEvent, new RoutedEventHandler(KeyButton_Click), true);
            KeyArea.AddHandler(Button.ContextMenuOpeningEvent, new RoutedEventHandler(KeyButton_ContextMenuOpening), true);
            KeyArea.AddHandler(Button.MouseLeftButtonDownEvent, new RoutedEventHandler(KeyButton_MouseLeftButtonDown), true);
            KeyArea.AddHandler(Button.MouseLeftButtonUpEvent, new RoutedEventHandler(KeyButton_MouseButtonUp), true);
            KeyArea.AddHandler(Button.MouseRightButtonDownEvent, new RoutedEventHandler(KeyButton_MouseRightButtonDown), true);
            KeyArea.AddHandler(Button.MouseRightButtonUpEvent, new RoutedEventHandler(KeyButton_MouseButtonUp), true);
            KeyArea.AddHandler(Button.MouseMoveEvent, new RoutedEventHandler(KeyButton_MouseMove), true);
            KeyArea.AddHandler(Button.DropEvent, new RoutedEventHandler(KeyButton_Drop), true);
        }
        // 閉じる処理をキャンセルして非表示にする
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            _vm.MainWindowVisibility = Visibility.Collapsed;
        }
        // ウィンドウが非アクティブになったら非表示にする
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            Keyboard.Focus(KeyArea);
            _vm.MainWindowVisibility = Visibility.Collapsed;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            Keyboard.Focus(this);
        }

        private void KeyArea_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _isKeyAreaFocus = true;
        }

        private void KeyArea_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _isKeyAreaFocus = false;
        }
        internal void SetKeyAreaFocus()
        {
            KeyArea.Focus();
        }
        internal void SetKeyFocus(string key)
        {
            var btn = FindName(key);
            if (btn != null)
            {
                ((Button)btn).Focus();
            };
        }

        // 背景クリック
        private void BackgroundArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _vm.MainWindowVisibility = Visibility.Collapsed;
        }

        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            //Debug.Print("KeyButton_Click");
            string keyName = ((Button)e.Source).Name;
            _vm.CurrentMap[keyName].Action();
            _isMouseDown = false;
        }

        // 右クリック
        private void KeyButton_ContextMenuOpening(object sender, RoutedEventArgs e)
        {
            //Debug.Print("KeyButton_ContextMenuOpening");
            string keyName = ((Button)e.Source).Name;
            EditView = new EditWindow(_vm, keyName);
            _vm.MainWindowVisibility = Visibility.Collapsed;
            EditView.Show();
        }

        // ドラッグ開始処理
        private void KeyButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            _isMouseDown = true;
            MouseEventArgs args = e as MouseEventArgs;
            _point = args.GetPosition(KeyArea);
            _DragEffect = DragDropEffects.Move;
        }
        private void KeyButton_MouseRightButtonDown(object sender, RoutedEventArgs e)
        {
            _isMouseDown = true;
            MouseEventArgs args = e as MouseEventArgs;
            _point = args.GetPosition(KeyArea);
            _DragEffect = DragDropEffects.Copy;
        }
        private void KeyButton_MouseButtonUp(object sender, RoutedEventArgs e)
        {
            _isMouseDown = false;
            if (_isKeyAreaFocus)
            {
                SetKeyAreaFocus();
            }
        }


        private void KeyButton_MouseMove(object sender, RoutedEventArgs e)
        {
            if (_isMouseDown)
            {
                //Debug.Print("KeyButton_MouseMove");
                Point pt = (e as MouseEventArgs).GetPosition(KeyArea);
                if (_point.X != pt.X || _point.Y != pt.Y)
                {
                    _isMouseDown = false;
                    Button btn = e.Source as Button;
                    Key btnData = btn.DataContext as Key;
                    if (btnData.Name != null)
                    {
                        //Debug.Print("KeyButton_Drag");
                        _DragSrcMap = _vm.CurrentMapName;
                        _DragSrcMod = _vm.CurrentMod;
                        _DragSrcKey = btn.Name;
                        DragDrop.DoDragDrop(btn, btn.Name, DragDropEffects.All);
                        //Debug.Print("dragEnd");
                    }
                }
            }
        }

        private void KeyButton_Drop(object sender, RoutedEventArgs e)
        {
            string dstmap = _vm.CurrentMapName;
            string dstmod = _vm.CurrentMod;
            string dstkey = ((Button)((DragEventArgs)e).Source).Name;


            if (_DragSrcKey != dstkey || _DragSrcMap != dstmap || _DragSrcMod != dstmod)
            {
                switch (_DragEffect)
                {
                    case DragDropEffects.Copy:
                        _vm.CurrentMap[dstkey] = _vm.Maps[_DragSrcMap][_DragSrcMod][_DragSrcKey];
                        break;

                    case DragDropEffects.Move:
                        Key tempkey = _vm.CurrentMap[dstkey].Clone();
                        _vm.CurrentMap[dstkey] = _vm.Maps[_DragSrcMap][_DragSrcMod][_DragSrcKey];
                        _vm.Maps[_DragSrcMap][_DragSrcMod][_DragSrcKey] = tempkey;
                        break;
                }
            } 
            else
            {
                _vm.CurrentMap[_DragSrcKey].Action();
            }
            KeyArea.Focus();

        }

        private void ChangeMapAnimation(object sender, EventArgs e)
        {
            //Debug.Print("changemap");
            _vm.CurrentMap = _vm.Maps[_vm.CurrentMapName][_vm.CurrentMod];
            _vm.IsChangeMap = false;
        }
    }
}
