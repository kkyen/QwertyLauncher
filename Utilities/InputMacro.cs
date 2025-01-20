using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;
using static QwertyLauncher.InputHook;
using static QwertyLauncher.Utilities.Easing;
namespace QwertyLauncher
{
    internal class InputMacro
    {
        private Task _task;
        private bool _cancel;
        


        internal async void Start(string macro, int count, double speed)
        {
            _cancel = false;
            App.State = "macroPlaying";

            /// マクロ実行中はキーボードの修飾キーを無効化
            foreach (var mod in new string[] {
                    "LControl",
                    "LShift",
                    "LMenu",
                    "RControl",
                    "RShift",
                    "RMenu"
            })
            {
                Input input = new Input { Type = InputFlags.KEYBOARD };
                input.ui.Keyboard.Flags = KeyEventFlags.KEYUP;
                input.ui.Keyboard.VirtualKey = VirtualKeyConverter.GetCode(mod);
                input.ui.Keyboard.ScanCode = (short)NativeMethods.MapVirtualKey(input.ui.Keyboard.VirtualKey, 0);
                input.ui.Keyboard.ExtraInfo = (IntPtr)WM_APP;
                NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
            }

            /// マクロ実行
            _task = Exec(macro, count, speed);
            await _task;

            /// マクロ実行後はキーボードの修飾キーを復元
            foreach ( var mod in App.Context.CurrentMod.Split(','))
            {
                Input input = new Input { Type = InputFlags.KEYBOARD };
                input.ui.Keyboard.Flags = KeyEventFlags.KEYDOWN;
                input.ui.Keyboard.VirtualKey = VirtualKeyConverter.GetCode(mod);
                input.ui.Keyboard.ScanCode = (short)NativeMethods.MapVirtualKey(input.ui.Keyboard.VirtualKey, 0);
                input.ui.Keyboard.ExtraInfo = (IntPtr)WM_APP;
                NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
            }

            App.State = "ready";
        }
        internal void Cancel()
        {
            _cancel = true;
        }
        private async Task Exec(string macro, int count, double speed)
        {
            string[] lines = macro.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            int c = count;
            if (count == 0) c = 1;
            while (0 < c)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                foreach (string line in lines)
                {
                    if (_cancel) return;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] seq = line.Split(',');

                    if (!long.TryParse(seq[0], out long ms)) continue;
                    if (!Enum.TryParse(seq[1], out InputFlags type)) continue;

                    ms = (long)(ms / speed);

                    if (seq[2] != "ABSOLUTESTROKE" && seq[2] != "RELATIVESTROKE")
                    {
                        while (sw.ElapsedMilliseconds < ms)
                        {
                            if (_cancel) return;
                            await Task.Yield();
                        }
                    }

                    Input input = new Input { Type = type };

                    /// キーボード
                    if (type == InputFlags.KEYBOARD)
                    {
                        if (!Enum.TryParse(seq[2], out KeyEventFlags flags)) continue;

                        short vk = VirtualKeyConverter.GetCode(seq[3]);
                        if (vk == 0) continue;

                        input.ui.Keyboard.Flags = flags;
                        input.ui.Keyboard.VirtualKey = vk;
                        input.ui.Keyboard.ScanCode = (short)NativeMethods.MapVirtualKey(vk, 0);
                        input.ui.Keyboard.ExtraInfo = (IntPtr)WM_APP;

                        NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
                    }

                    /// マウス
                    if (type == InputFlags.MOUSE)
                    {
                        if (!Enum.TryParse(seq[2], out MouseEventFlags flags)) continue;
                        input.ui.Mouse.Flags = flags;
                        input.ui.Mouse.ExtraInfo = (IntPtr)WM_APP;

                        /// マウスボタン・ホイール
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
                            NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
                        }

                        /// 移動
                        if (flags == MouseEventFlags.MOVE)
                        {
                            input.ui.Mouse.X = int.Parse(seq[3]);
                            input.ui.Mouse.Y = int.Parse(seq[4]);
                            NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
                        }

                        ///
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

                            switch (seq[2])
                            {
                                /// 移動
                                case "ABSOLUTEMOVE":
                                case "RELATIVEMOVE":
                                    input.ui.Mouse.X = AbsX(x);
                                    input.ui.Mouse.Y = AbsY(y);
                                    NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
                                    break;

                                /// ストローク
                                case "ABSOLUTESTROKE":
                                case "RELATIVESTROKE":
                                    double startMs = sw.ElapsedMilliseconds;
                                    int startX = curX;
                                    int startY = curY;
                                    double diffMs = ms - startMs;
                                    double diffX = x - curX;
                                    double diffY = y - curY;
                                    while (sw.ElapsedMilliseconds < ms)
                                    {
                                        if (_cancel) return;
                                        var prog = CubicInOut(((float)(sw.ElapsedMilliseconds - startMs)), ((float)diffMs), 0, 1);
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
                                    break;
                            }
                        }
                    }
                }
                sw.Reset();
                if (count != 0) c -= 1;
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
