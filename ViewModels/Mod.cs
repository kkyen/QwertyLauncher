using System;

namespace QwertyLauncher
{
    public class Mod
    {
        /// <summary> ****************************************
        /// Properties
        /// </summary>****************************************
        public string Name { get; set; }
        private ViewModel _vm;

        /// <summary>
        /// Keys型をとりあえず全て定義
        /// https://learn.microsoft.com/ja-jp/dotnet/api/system.windows.forms.keys
        /// </summary>

        public Key LButton { get; set; }
        public Key RButton { get; set; }
        public Key Cancel { get; set; }
        public Key MButton { get; set; }
        public Key XButton1 { get; set; }
        public Key XButton2 { get; set; }
        public Key Back { get; set; }
        public Key Tab { get; set; }
        public Key Clear { get; set; }
        public Key Return { get; set; }
        public Key Shift { get; set; }
        public Key Control { get; set; }
        public Key Menu { get; set; }
        public Key Pause { get; set; }
        public Key Capital { get; set; }
        public Key Kana { get; set; }
        public Key Junja { get; set; }
        public Key Final { get; set; }
        public Key Kanji { get; set; }
        public Key Escape { get; set; }
        public Key Convert { get; set; }
        public Key NonConvert { get; set; }
        public Key Accept { get; set; }
        public Key ModeChange { get; set; }
        public Key Space { get; set; }
        public Key Prior { get; set; }
        public Key Next { get; set; }
        public Key End { get; set; }
        public Key Home { get; set; }
        public Key Left { get; set; }
        public Key Up { get; set; }
        public Key Right { get; set; }
        public Key Down { get; set; }
        public Key Select { get; set; }
        public Key Print { get; set; }
        public Key Execute { get; set; }
        public Key Snapshot { get; set; }
        public Key Insert { get; set; }
        public Key Delete { get; set; }
        public Key Help { get; set; }
        public Key D0 { get; set; }
        public Key D1 { get; set; }
        public Key D2 { get; set; }
        public Key D3 { get; set; }
        public Key D4 { get; set; }
        public Key D5 { get; set; }
        public Key D6 { get; set; }
        public Key D7 { get; set; }
        public Key D8 { get; set; }
        public Key D9 { get; set; }
        public Key A { get; set; }
        public Key B { get; set; }
        public Key C { get; set; }
        public Key D { get; set; }
        public Key E { get; set; }
        public Key F { get; set; }
        public Key G { get; set; }
        public Key H { get; set; }
        public Key I { get; set; }
        public Key J { get; set; }
        public Key K { get; set; }
        public Key L { get; set; }
        public Key M { get; set; }
        public Key N { get; set; }
        public Key O { get; set; }
        public Key P { get; set; }
        public Key Q { get; set; }
        public Key R { get; set; }
        public Key S { get; set; }
        public Key T { get; set; }
        public Key U { get; set; }
        public Key V { get; set; }
        public Key W { get; set; }
        public Key X { get; set; }
        public Key Y { get; set; }
        public Key Z { get; set; }
        public Key LWin { get; set; }
        public Key RWin { get; set; }
        public Key Apps { get; set; }
        public Key Sleep { get; set; }
        public Key Numpad0 { get; set; }
        public Key Numpad1 { get; set; }
        public Key Numpad2 { get; set; }
        public Key Numpad3 { get; set; }
        public Key Numpad4 { get; set; }
        public Key Numpad5 { get; set; }
        public Key Numpad6 { get; set; }
        public Key Numpad7 { get; set; }
        public Key Numpad8 { get; set; }
        public Key Numpad9 { get; set; }
        public Key Multiply { get; set; }
        public Key Add { get; set; }
        public Key Separator { get; set; }
        public Key Subtract { get; set; }
        public Key Decimal { get; set; }
        public Key Divide { get; set; }
        public Key F1 { get; set; }
        public Key F2 { get; set; }
        public Key F3 { get; set; }
        public Key F4 { get; set; }
        public Key F5 { get; set; }
        public Key F6 { get; set; }
        public Key F7 { get; set; }
        public Key F8 { get; set; }
        public Key F9 { get; set; }
        public Key F10 { get; set; }
        public Key F11 { get; set; }
        public Key F12 { get; set; }
        public Key F13 { get; set; }
        public Key F14 { get; set; }
        public Key F15 { get; set; }
        public Key F16 { get; set; }
        public Key F17 { get; set; }
        public Key F18 { get; set; }
        public Key F19 { get; set; }
        public Key F20 { get; set; }
        public Key F21 { get; set; }
        public Key F22 { get; set; }
        public Key F23 { get; set; }
        public Key F24 { get; set; }
        public Key NumLock { get; set; }
        public Key Scroll { get; set; }
        public Key LShift { get; set; }
        public Key RShift { get; set; }
        public Key LControl { get; set; }
        public Key RControl { get; set; }
        public Key LMenu { get; set; }
        public Key RMenu { get; set; }
        public Key BrowserBack { get; set; }
        public Key BrowserForward { get; set; }
        public Key BrowserRefresh { get; set; }
        public Key BrowserStop { get; set; }
        public Key BrowserSearch { get; set; }
        public Key BrowserFavorites { get; set; }
        public Key BrowserHome { get; set; }
        public Key VolumeMute { get; set; }
        public Key VolumeDown { get; set; }
        public Key VolumeUp { get; set; }
        public Key MediaNextTrack { get; set; }
        public Key MediaPrevTrack { get; set; }
        public Key MediaStop { get; set; }
        public Key MediaPlayPause { get; set; }
        public Key LaunchMail { get; set; }
        public Key SelectMedia { get; set; }
        public Key LaunchApplication1 { get; set; }
        public Key LaunchApplication2 { get; set; }
        public Key Oem1 { get; set; }
        public Key OemPlus { get; set; }
        public Key OemComma { get; set; }
        public Key OemMinus { get; set; }
        public Key OemPeriod { get; set; }
        public Key Oem2 { get; set; }
        public Key Oem3 { get; set; }
        public Key Oem4 { get; set; }
        public Key Oem5 { get; set; }
        public Key Oem6 { get; set; }
        public Key Oem7 { get; set; }
        public Key Oem8 { get; set; }
        public Key OemAx { get; set; }
        public Key Oem102 { get; set; }
        public Key IcoHelp { get; set; }
        public Key Ico00 { get; set; }
        public Key ProcessKey { get; set; }
        public Key IcoClear { get; set; }
        public Key Packet { get; set; }
        public Key OemReset { get; set; }
        public Key OemJump { get; set; }
        public Key OemPa1 { get; set; }
        public Key OemPa2 { get; set; }
        public Key OemPa3 { get; set; }
        public Key OemWsctrl { get; set; }
        public Key OemCusel { get; set; }
        public Key OemAttn { get; set; }
        public Key OemFinish { get; set; }
        public Key OemCopy { get; set; }
        public Key OemAuto { get; set; }
        public Key OemEnlw { get; set; }
        public Key OemBacktab { get; set; }
        public Key Attn { get; set; }
        public Key Crsel { get; set; }
        public Key Exsel { get; set; }
        public Key EraseEof { get; set; }
        public Key Play { get; set; }
        public Key Zoom { get; set; }
        public Key NoName { get; set; }
        public Key Pa1 { get; set; }
        public Key OemClear { get; set; }


        /// コンストラクタ
        public Mod(ViewModel vm) {
            _vm = vm;

            LButton = new Key(_vm);
            RButton = new Key(_vm);
            Cancel = new Key(_vm);
            MButton = new Key(_vm);
            XButton1 = new Key(_vm);
            XButton2 = new Key(_vm);
            Back = new Key(_vm);
            Tab = new Key(_vm);
            Clear = new Key(_vm);
            Return = new Key(_vm);
            Shift = new Key(_vm);
            Control = new Key(_vm);
            Menu = new Key(_vm);
            Pause = new Key(_vm);
            Capital = new Key(_vm);
            Kana = new Key(_vm);
            Junja = new Key(_vm);
            Final = new Key(_vm);
            Kanji = new Key(_vm);
            Escape = new Key(_vm);
            Convert = new Key(_vm);
            NonConvert = new Key(_vm);
            Accept = new Key(_vm);
            ModeChange = new Key(_vm);
            Space = new Key(_vm);
            Prior = new Key(_vm);
            Next = new Key(_vm);
            End = new Key(_vm);
            Home = new Key(_vm);
            Left = new Key(_vm);
            Up = new Key(_vm);
            Right = new Key(_vm);
            Down = new Key(_vm);
            Select = new Key(_vm);
            Print = new Key(_vm);
            Execute = new Key(_vm);
            Snapshot = new Key(_vm);
            Insert = new Key(_vm);
            Delete = new Key(_vm);
            Help = new Key(_vm);
            D0 = new Key(_vm);
            D1 = new Key(_vm);
            D2 = new Key(_vm);
            D3 = new Key(_vm);
            D4 = new Key(_vm);
            D5 = new Key(_vm);
            D6 = new Key(_vm);
            D7 = new Key(_vm);
            D8 = new Key(_vm);
            D9 = new Key(_vm);
            A = new Key(_vm);
            B = new Key(_vm);
            C = new Key(_vm);
            D = new Key(_vm);
            E = new Key(_vm);
            F = new Key(_vm);
            G = new Key(_vm);
            H = new Key(_vm);
            I = new Key(_vm);
            J = new Key(_vm);
            K = new Key(_vm);
            L = new Key(_vm);
            M = new Key(_vm);
            N = new Key(_vm);
            O = new Key(_vm);
            P = new Key(_vm);
            Q = new Key(_vm);
            R = new Key(_vm);
            S = new Key(_vm);
            T = new Key(_vm);
            U = new Key(_vm);
            V = new Key(_vm);
            W = new Key(_vm);
            X = new Key(_vm);
            Y = new Key(_vm);
            Z = new Key(_vm);
            LWin = new Key(_vm);
            RWin = new Key(_vm);
            Apps = new Key(_vm);
            Sleep = new Key(_vm);
            Numpad0 = new Key(_vm);
            Numpad1 = new Key(_vm);
            Numpad2 = new Key(_vm);
            Numpad3 = new Key(_vm);
            Numpad4 = new Key(_vm);
            Numpad5 = new Key(_vm);
            Numpad6 = new Key(_vm);
            Numpad7 = new Key(_vm);
            Numpad8 = new Key(_vm);
            Numpad9 = new Key(_vm);
            Multiply = new Key(_vm);
            Add = new Key(_vm);
            Separator = new Key(_vm);
            Subtract = new Key(_vm);
            Decimal = new Key(_vm);
            Divide = new Key(_vm);
            F1 = new Key(_vm);
            F2 = new Key(_vm);
            F3 = new Key(_vm);
            F4 = new Key(_vm);
            F5 = new Key(_vm);
            F6 = new Key(_vm);
            F7 = new Key(_vm);
            F8 = new Key(_vm);
            F9 = new Key(_vm);
            F10 = new Key(_vm);
            F11 = new Key(_vm);
            F12 = new Key(_vm);
            F13 = new Key(_vm);
            F14 = new Key(_vm);
            F15 = new Key(_vm);
            F16 = new Key(_vm);
            F17 = new Key(_vm);
            F18 = new Key(_vm);
            F19 = new Key(_vm);
            F20 = new Key(_vm);
            F21 = new Key(_vm);
            F22 = new Key(_vm);
            F23 = new Key(_vm);
            F24 = new Key(_vm);
            NumLock = new Key(_vm);
            Scroll = new Key(_vm);
            LShift = new Key(_vm);
            RShift = new Key(_vm);
            LControl = new Key(_vm);
            RControl = new Key(_vm);
            LMenu = new Key(_vm);
            RMenu = new Key(_vm);
            BrowserBack = new Key(_vm);
            BrowserForward = new Key(_vm);
            BrowserRefresh = new Key(_vm);
            BrowserStop = new Key(_vm);
            BrowserSearch = new Key(_vm);
            BrowserFavorites = new Key(_vm);
            BrowserHome = new Key(_vm);
            VolumeMute = new Key(_vm);
            VolumeDown = new Key(_vm);
            VolumeUp = new Key(_vm);
            MediaNextTrack = new Key(_vm);
            MediaPrevTrack = new Key(_vm);
            MediaStop = new Key(_vm);
            MediaPlayPause = new Key(_vm);
            LaunchMail = new Key(_vm);
            SelectMedia = new Key(_vm);
            LaunchApplication1 = new Key(_vm);
            LaunchApplication2 = new Key(_vm);
            Oem1 = new Key(_vm);
            OemPlus = new Key(_vm);
            OemComma = new Key(_vm);
            OemMinus = new Key(_vm);
            OemPeriod = new Key(_vm);
            Oem2 = new Key(_vm);
            Oem3 = new Key(_vm);
            Oem4 = new Key(_vm);
            Oem5 = new Key(_vm);
            Oem6 = new Key(_vm);
            Oem7 = new Key(_vm);
            Oem8 = new Key(_vm);
            OemAx = new Key(_vm);
            Oem102 = new Key(_vm);
            IcoHelp = new Key(_vm);
            Ico00 = new Key(_vm);
            ProcessKey = new Key(_vm);
            IcoClear = new Key(_vm);
            Packet = new Key(_vm);
            OemReset = new Key(_vm);
            OemJump = new Key(_vm);
            OemPa1 = new Key(_vm);
            OemPa2 = new Key(_vm);
            OemPa3 = new Key(_vm);
            OemWsctrl = new Key(_vm);
            OemCusel = new Key(_vm);
            OemAttn = new Key(_vm);
            OemFinish = new Key(_vm);
            OemCopy = new Key(_vm);
            OemAuto = new Key(_vm);
            OemEnlw = new Key(_vm);
            OemBacktab = new Key(_vm);
            Attn = new Key(_vm);
            Crsel = new Key(_vm);
            Exsel = new Key(_vm);
            EraseEof = new Key(_vm);
            Play = new Key(_vm);
            Zoom = new Key(_vm);
            NoName = new Key(_vm);
            Pa1 = new Key(_vm);
            OemClear = new Key(_vm);

        }

        // Indexer
        // **************************************************
        public Key this[string propertyName]
        {
            get
            {
                return (Key)typeof(Mod).GetProperty(propertyName).GetValue(this);
            }
            set
            {
                typeof(Mod).GetProperty(propertyName).SetValue(this, value);
                ModUpdateEventHandler(this, new ModUpdateEventArgs()
                {
                    modName = Name,
                    keyName = propertyName
                });
            }
        }

        // Event
        // **************************************************
        internal event EventHandler<ModUpdateEventArgs> ModUpdateEventHandler;

        // SubClass
        // **************************************************
        public class ModUpdateEventArgs
        {
            public string modName;
            public string keyName;
        }

    }
}
