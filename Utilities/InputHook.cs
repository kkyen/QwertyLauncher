using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QwertyLauncher
{
    internal class InputHook
    {

        /// <summary>
        /// InputHook
        /// </summary>

        private const int WH_KEYBOARD_LL = 0x000D;
        private const int WH_MOUSE_LL = 0x000E;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_XBUTTONDOWN = 0x020B;
        private const int WM_XBUTTONUP = 0x020C;
        private const int WM_MOUSEHWHEEL = 0x020E;

        [StructLayout(LayoutKind.Sequential)]
        internal class KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public KBDLLHOOKSTRUCT_FLAGS flags;
            public int time;
            public int dwExtraInfo;
        }
        [Flags]
        internal enum KBDLLHOOKSTRUCT_FLAGS : int
        {
            KEYEVENTF_EXTENDEDKEY = 0x0001,
            KEYEVENTF_KEYUP = 0x0002,
            KEYEVENTF_UNICODE = 0x0004,
            KEYEVENTF_SCANCODE = 0x0008,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MSLLHOOKSTRUCT
        {
            public MSLLHOOKSTRUCT_POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct MSLLHOOKSTRUCT_POINT
        {
            public int x;
            public int y;
        }

        private readonly NativeMethods.HookCallback _keyboardHookProc;
        private readonly NativeMethods.SafeHookHandle _keyboardHookHandle;

        private readonly NativeMethods.HookCallback _mouseHookProc;
        private readonly NativeMethods.SafeHookHandle _mouseHookHandle;

        internal InputHook()
        {
            if (_keyboardHookHandle == null)
            {
                _keyboardHookProc = KeyboardProc;
                _keyboardHookHandle = NativeMethods.SetWindowsHookEx(
                    WH_KEYBOARD_LL,
                    _keyboardHookProc,
                    NativeMethods.GetModuleHandle(null), 0);
            }
            if (_mouseHookHandle == null)
            {
                _mouseHookProc = MouseProc;
                _mouseHookHandle = NativeMethods.SetWindowsHookEx(
                    WH_MOUSE_LL,
                    _mouseHookProc,
                    NativeMethods.GetModuleHandle(null), 0);
            }
        }

        internal event EventHandler<KeyboardHookEventArgs> OnKeyboardHookEvent = delegate { };
        internal class KeyboardHookEventArgs
        {
            internal string Msg {  get; set; }
            internal string Key { get; set; }
            internal int VkCode { get; set; }
            internal int ScanCode { get; set;}
            internal KBDLLHOOKSTRUCT_FLAGS Flags { get; set; }
            internal int Time {  get; set; }
            internal int DwExtraInfo { get; set; }
            internal bool Handled { get; set; } = false;
        }
        private IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var kbdllhookstruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
            string keyname = VirtualKeyConverter.GetName(kbdllhookstruct.vkCode);
            Debug.Print(string.Format("ncode={0} vkcode={1} scancode={2} flags={3} time={4} dwExtraInfo={5} keys={6}",
                nCode,
                kbdllhookstruct.vkCode,
                kbdllhookstruct.scanCode,
                kbdllhookstruct.flags,
                kbdllhookstruct.time,
                kbdllhookstruct.dwExtraInfo,
                keyname
                )); 
            var args = new KeyboardHookEventArgs
            {
                Key = keyname,
                VkCode = kbdllhookstruct.vkCode,
                ScanCode = kbdllhookstruct.scanCode,
                Flags = kbdllhookstruct.flags,
                Time = kbdllhookstruct.time,
                DwExtraInfo = kbdllhookstruct.dwExtraInfo
            };
            if (nCode >= 0)
            {
                if ((int)wParam == WM_KEYDOWN) args.Msg = "KEYDOWN";
                if ((int)wParam == WM_KEYUP) args.Msg = "KEYUP";
                if ((int)wParam == WM_SYSKEYDOWN) args.Msg = "KEYDOWN";
                if ((int)wParam == WM_SYSKEYUP) args.Msg = "KEYUP";
                if (null != args.Msg) OnKeyboardHookEvent(this, args);
            }
            return args.Handled ? (IntPtr)1 : NativeMethods.CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
        }

        internal event EventHandler<MouseHookEventArgs> OnMouseHookEvent = delegate { };
        internal class MouseHookEventArgs
        {
            internal string Msg { get; set; }
            internal int PosX { get; set; }
            internal int PosY { get; set; }
            internal int Time { get; set; }
            internal int Data { get; set; }
            internal int ForegroundWindowId { get; set; }
            internal bool Handled { get; set; } = false;
        }
        private IntPtr MouseProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var msllhookstruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            NativeMethods.GetWindowThreadProcessId(NativeMethods.GetForegroundWindow(), out int pid);
            var args = new MouseHookEventArgs
            {
                PosX = msllhookstruct.pt.x,
                PosY = msllhookstruct.pt.y,
                Time = msllhookstruct.time,
                Data = msllhookstruct.mouseData >> 16,
                ForegroundWindowId = pid
            };
            if (nCode >= 0)
            {
                if ((int)wParam == WM_MOUSEMOVE) args.Msg = "MOVE";
                if ((int)wParam == WM_LBUTTONDOWN) args.Msg = "LEFTDOWN";
                if ((int)wParam == WM_LBUTTONUP) args.Msg = "LEFTUP";
                if ((int)wParam == WM_RBUTTONDOWN) args.Msg = "RIGHTDOWN";
                if ((int)wParam == WM_RBUTTONUP) args.Msg = "RIGHTUP";
                if ((int)wParam == WM_MBUTTONDOWN) args.Msg = "MIDDLEDOWN";
                if ((int)wParam == WM_MBUTTONUP) args.Msg = "MIDDLEUP";
                if ((int)wParam == WM_MOUSEWHEEL) args.Msg = "WHEEL";
                if ((int)wParam == WM_XBUTTONDOWN) args.Msg = "XDOWN";
                if ((int)wParam == WM_XBUTTONUP) args.Msg = "XUP";
                if ((int)wParam == WM_MOUSEHWHEEL) args.Msg = "HWHEEL";
                if (null != args.Msg)  OnMouseHookEvent(this, args);
            }
            return args.Handled ? (IntPtr)1 : NativeMethods.CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
        }

        internal static class NativeMethods
        {


            internal class SafeHookHandle : SafeHandle
            {
                private static class NativeMethods
                {
                    [DllImport("user32.dll")]
                    [return: MarshalAs(UnmanagedType.Bool)]
                    internal static extern bool UnhookWindowsHookEx(IntPtr hhk);
                }
                public SafeHookHandle()
                    : base(IntPtr.Zero, true)
                {
                }
                public SafeHookHandle(IntPtr preexistingHandle, bool ownsHandle = true)
                    : base(IntPtr.Zero, ownsHandle)
                {
                    SetHandle(preexistingHandle);
                }
                public override bool IsInvalid => handle == IntPtr.Zero;
                protected override bool ReleaseHandle() => NativeMethods.UnhookWindowsHookEx(handle);
            }



            internal delegate IntPtr HookCallback(
                int nCode,
                IntPtr wParam,
                IntPtr lParam);

            [DllImport("user32.dll")]
            internal static extern SafeHookHandle SetWindowsHookEx(
                int idHook,
                HookCallback lpfn,
                IntPtr hMod,
                int dwThreadId);

            [DllImport("user32.dll")]
            internal static extern IntPtr CallNextHookEx(
                SafeHookHandle hhk,
                int nCode,
                IntPtr wParam,
                IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern IntPtr GetModuleHandle(
                [MarshalAs(UnmanagedType.LPWStr), In] string lpModuleName);

            [DllImport("user32.dll")]
            internal static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            internal static extern int GetWindowThreadProcessId(
                IntPtr hWnd,
                out int lpdwProcessId);
        }
    }
}
