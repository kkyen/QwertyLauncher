using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;
using static QwertyLauncher.Views.InputHook;

namespace QwertyLauncher.Views
{
    internal class InputMacro
    {
        private Task _task;
        private bool _cancel;
        internal bool IsRunning = false;
        internal event EventHandler OnStartMacroEvent = delegate { };
        internal event EventHandler OnStopMacroEvent = delegate { };

        internal async void Start(string macro, int macrocount)
        {
            _cancel = false;
            IsRunning = true;
            //App.TaskTrayIcon.ChangeIcon(App.IconExecute);
            OnStartMacroEvent(this, EventArgs.Empty);
            _task = Exec(macro, macrocount);
            await _task;
            IsRunning = false;
            OnStopMacroEvent(this, EventArgs.Empty);
            //App.TaskTrayIcon.TrayIcon.Icon = App.TaskTrayIcon.IconNormal;
        }
        internal void Cancel()
        {
            _cancel = true;
        }
        private async Task Exec(string macro, int macrocount)
        {
            string[] lines = macro.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            int c = macrocount;
            if (macrocount == 0) c = 1;
            while (0 < c)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                foreach (string line in lines)
                {
                    if (_cancel) return;
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] seq = line.Split(',');

                        if (
                            long.TryParse(seq[0], out long ms) &&
                            Enum.TryParse(seq[1], out InputFlags type)
                        )
                        {

                            Input input = new Input
                            {
                                Type = type
                            };

                            if (type == InputFlags.KEYBOARD)
                            {
                                if (
                                    Enum.TryParse(seq[2], out KeyEventFlags flags) &&
                                    Enum.TryParse(seq[3], true, out Keys key)
                                )
                                {
                                    input.ui.Keyboard.Flags = flags;
                                    input.ui.Keyboard.VirtualKey = (short)(int)key;
                                    input.ui.Keyboard.ScanCode = (short)NativeMethods.MapVirtualKey(input.ui.Keyboard.VirtualKey, 0);

                                    while (sw.ElapsedMilliseconds < ms) await Task.Yield();
                                    NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
                                }
                            }


                            if (type == InputFlags.MOUSE)
                            {
                                if (Enum.TryParse(seq[2], out MouseEventFlags flags))
                                {
                                    input.ui.Mouse.Flags = flags;
                                    // マウスボタン
                                    if (
                                        flags == MouseEventFlags.LEFTDOWN ||
                                        flags == MouseEventFlags.LEFTUP ||
                                        flags == MouseEventFlags.RIGHTDOWN ||
                                        flags == MouseEventFlags.RIGHTUP ||
                                        flags == MouseEventFlags.MIDDLEDOWN ||
                                        flags == MouseEventFlags.MIDDLEUP ||
                                        flags == MouseEventFlags.WHEEL ||
                                        flags == MouseEventFlags.XDOWN ||
                                        flags == MouseEventFlags.XUP ||
                                        flags == MouseEventFlags.HWHEEL
                                    )
                                    {
                                        input.ui.Mouse.Data = int.Parse(seq[3]);
                                        while (sw.ElapsedMilliseconds < ms)
                                        {
                                            if (_cancel) return;
                                            await Task.Yield();
                                        }
                                        NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
                                    }

                                    if (flags == MouseEventFlags.MOVE)
                                    {
                                        input.ui.Mouse.X = int.Parse(seq[3]);
                                        input.ui.Mouse.Y = int.Parse(seq[4]);


                                        while (sw.ElapsedMilliseconds < ms)
                                        {
                                            if (_cancel) return;
                                            await Task.Yield();
                                        }
                                        NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
                                    }

                                    if (flags == MouseEventFlags.ABSOLUTEMOVE)
                                    {

                                        int x = int.Parse(seq[3]);
                                        int y = int.Parse(seq[4]);
                                        int curX = Cursor.Position.X;
                                        int curY = Cursor.Position.Y;

                                        // 相対位置指定は現在の座標を加算
                                        if (
                                            seq[2] == "RELATIVESTROKE" ||
                                            seq[2] == "RELATIVEMOVE"
                                        )
                                        {
                                            x += curX;
                                            y += curY;
                                        }

                                        // 移動
                                        if (
                                            seq[2] == "ABSOLUTEMOVE" ||
                                            seq[2] == "RELATIVEMOVE"
                                        )
                                        {
                                            input.ui.Mouse.X = AbsX(x);
                                            input.ui.Mouse.Y = AbsY(y);
                                            while (sw.ElapsedMilliseconds < ms)
                                            {
                                                if (_cancel) return;
                                                await Task.Yield();
                                            }
                                            NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
                                        }

                                        // ストローク
                                        if (
                                            seq[2] == "ABSOLUTESTROKE" ||
                                            seq[2] == "RELATIVESTROKE"
                                        )
                                        {
                                            double startMs = sw.ElapsedMilliseconds;
                                            int startX = curX;
                                            int startY = curY;
                                            double diffMs = ms - startMs;
                                            double diffX = x - curX;
                                            double diffY = y - curY;
                                            while (sw.ElapsedMilliseconds < ms)
                                            {
                                                if (_cancel) return;
                                                double prog = (sw.ElapsedMilliseconds - startMs) / diffMs;
                                                curX = startX + (int)Math.Round(diffX * prog);
                                                curY = startY + (int)Math.Round(diffY * prog);
                                                input.ui.Mouse.X = AbsX(curX);
                                                input.ui.Mouse.Y = AbsY(curY);
                                                NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
                                                await Task.Yield();
                                            }
                                            if (_cancel) return;
                                            input.ui.Mouse.X = AbsX(x);
                                            input.ui.Mouse.Y = AbsY(y);
                                            NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                sw.Reset();
                if (macrocount != 0) c -= 1;
            }
        }


        [Flags] internal enum InputFlags
        {
            MOUSE = 0,                  // マウスイベント
            KEYBOARD = 1,               // キーボードイベント
            HARDWARE = 2,               // ハードウェアイベント
        }
        [Flags]
        internal enum KeyEventFlags
        {

            KEYDOWN = 0x0,          // キーを押す
            KEYUP = 0x2,            // キーを離す
            EXTENDEDKEY = 0x1,      // 拡張コード
        }
        [Flags]
        internal enum MouseEventFlags
        {

            MOVE = 0x0001,                 // マウスを移動する ※マウスの加速設定の影響を受ける
            ABSOLUTE = 0x8000,          // 絶対座標指定
            ABSOLUTEMOVE = MOVE | ABSOLUTE,
            RELATIVE = ABSOLUTEMOVE,         // 独自拡張 相対座標指定
            ABSOLUTESTROKE = ABSOLUTEMOVE,    // 独自拡張 中間座標を補完する絶対座標指定
            RELATIVESTROKE = ABSOLUTEMOVE,   // 独自拡張 中間座標を補完する相対座標指定
            LEFTDOWN = 0x0002,             // 左　ボタンを押す
            LEFTUP = 0x0004,               // 左　ボタンを離す
            RIGHTDOWN = 0x0008,            // 右　ボタンを押す
            RIGHTUP = 0x0010,             // 右　ボタンを離す
            MIDDLEDOWN = 0x0020,          // 中央ボタンを押す
            MIDDLEUP = 0x0040,            // 中央ボタンを離す
            XDOWN = 0x0080,            // 中央ボタンを離す
            XUP = 0x0100,            // 中央ボタンを離す
            WHEEL = 0x0800,              // ホイールを回転する
            HWHEEL = 0x1000,              // 水平ホイールを回転する
        }

        private int AbsX(int x)
        {
            int w = Screen.PrimaryScreen.Bounds.Width;
            return (x * 65536 + w - 1) / w;
        }
        private int AbsY(int y)
        {
            int h = Screen.PrimaryScreen.Bounds.Height;
            return (y * 65536 + h - 1) / h;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int X;
            public int Y;
            public int Data;
            public MouseEventFlags Flags;
            public int Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public short VirtualKey;
            public short ScanCode;
            public KeyEventFlags Flags;
            public int Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Input
        {
            public InputFlags Type;
            public InputUnion ui;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MouseInput Mouse;
            [FieldOffset(0)]
            public KeyboardInput Keyboard;
            [FieldOffset(0)]
            public HardwareInput Hardware;
        }
        internal static class NativeMethods
        {

            [DllImport("user32.dll")]
            internal static extern void SendInput(int nInputs, ref Input pInputs, int cbsize);

            [DllImport("user32.dll")]
            internal static extern short VkKeyScan(char ch);

            [DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
            internal static extern int MapVirtualKey(int wCode, int wMapType);
        }
    }
}
