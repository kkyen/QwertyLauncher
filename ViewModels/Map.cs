using System;

namespace QwertyLauncher
{
    public class Map
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

        public Key A { get; set; } //65 A キー
        public Key Add { get; set; } //107 Add キー
        public Key Alt { get; set; } //262144 Alt 修飾子キー
        public Key Apps { get; set; } //93 アプリケーション キー (Microsoft Natural Keyboard)
        public Key Attn { get; set; } //246 Attn キー
        public Key B { get; set; } //66 B キー
        public Key Back { get; set; } //8 BackSpace キー
        public Key BrowserBack { get; set; } //166 ブラウザーの戻るキー
        public Key BrowserFavorites { get; set; } //171 ブラウザーのお気に入りキー
        public Key BrowserForward { get; set; } //167 ブラウザーの進むキー
        public Key BrowserHome { get; set; } //172 ブラウザーのホーム キー
        public Key BrowserRefresh { get; set; } //168 ブラウザーの更新キー
        public Key BrowserSearch { get; set; } //170 ブラウザーの検索キー
        public Key BrowserStop { get; set; } //169 ブラウザーの中止キー
        public Key C { get; set; } //67 C キー
        public Key Cancel { get; set; } //3 Cancel キー
        public Key Capital { get; set; } //20 CAPS LOCK キー
        public Key CapsLock { get; set; } //20 CAPS LOCK キー
        public Key Clear { get; set; } //12 Clear キー
        public Key Control { get; set; } //131072 Ctrl 修飾子キー
        public Key ControlKey { get; set; } //17 CTRL キー
        public Key Crsel { get; set; } //247 Crsel キー
        public Key D { get; set; } //68 D キー
        public Key D0 { get; set; } //48 0 キー
        public Key D1 { get; set; } //49 1 キー
        public Key D2 { get; set; } //50 2 キー
        public Key D3 { get; set; } //51 3 キー
        public Key D4 { get; set; } //52 4 キー
        public Key D5 { get; set; } //53 5 キー
        public Key D6 { get; set; } //54 6 キー
        public Key D7 { get; set; } //55 7 キー
        public Key D8 { get; set; } //56 8 キー
        public Key D9 { get; set; } //57 9 キー
        public Key Decimal { get; set; } //110 小数点キー
        public Key Delete { get; set; } //46 DEL キー
        public Key Divide { get; set; } //111 除算記号 (/) キー
        public Key Down { get; set; } //40 ↓キー
        public Key E { get; set; } //69 E キー
        public Key End { get; set; } //35 End キー
        public Key Enter { get; set; } //13 Enter キー
        public Key EraseEof { get; set; } //249 Erase Eof キー
        public Key Escape { get; set; } //27 Esc キー
        public Key Execute { get; set; } //43 Execute キー
        public Key Exsel { get; set; } //248 Exsel キー
        public Key F { get; set; } //70 F キー
        public Key F1 { get; set; } //112 F1 キー
        public Key F10 { get; set; } //121 F10 キー
        public Key F11 { get; set; } //122 F11 キー
        public Key F12 { get; set; } //123 F12 キー
        public Key F13 { get; set; } //124 F13 キー
        public Key F14 { get; set; } //125 F14 キー
        public Key F15 { get; set; } //126 F15 キー
        public Key F16 { get; set; } //127 F16 キー
        public Key F17 { get; set; } //128 F17 キー
        public Key F18 { get; set; } //129 F18 キー
        public Key F19 { get; set; } //130 F19 キー
        public Key F2 { get; set; } //113 F2 キー
        public Key F20 { get; set; } //131 F20 キー
        public Key F21 { get; set; } //132 F21 キー
        public Key F22 { get; set; } //133 F22 キー
        public Key F23 { get; set; } //134 F23 キー
        public Key F24 { get; set; } //135 F24 キー
        public Key F3 { get; set; } //114 F3 キー
        public Key F4 { get; set; } //115 F4 キー
        public Key F5 { get; set; } //116 F5 キー
        public Key F6 { get; set; } //117 F6 キー
        public Key F7 { get; set; } //118 F7 キー
        public Key F8 { get; set; } //119 F8 キー
        public Key F9 { get; set; } //120 F9 キー
        public Key FinalMode { get; set; } //24 IME Final モード キー
        public Key G { get; set; } //71 G キー
        public Key H { get; set; } //72 H キー
        public Key HanguelMode { get; set; } //21 IME ハングル モード キー (互換性を保つために保持されていますHangulMode を使用します)
        public Key HangulMode { get; set; } //21 IME ハングル モード キー
        public Key HanjaMode { get; set; } //25 IME Hanja モード キー
        public Key Help { get; set; } //47 Help キー
        public Key Home { get; set; } //36 Home キー
        public Key I { get; set; } //73 I キー
        public Key IMEAccept { get; set; } //30 IME Accept キー (IMEAceept の代わりに使用します)
        public Key IMEAceept { get; set; } //30 IME Accept キー 互換性を維持するために残されています代わりに IMEAccept を使用してください
        public Key IMEConvert { get; set; } //28 IME 変換キー
        public Key IMEModeChange { get; set; } //31 IME モード変更キー
        public Key IMENonconvert { get; set; } //29 IME 無変換キー
        public Key Insert { get; set; } //45 INS キー
        public Key J { get; set; } //74 J キー
        public Key JunjaMode { get; set; } //23 IME Junja モード キー
        public Key K { get; set; } //75 K キー
        public Key KanaMode { get; set; } //21 IME かなモード キー
        public Key KanjiMode { get; set; } //25 IME 漢字モード キー
        public Key KeyCode { get; set; } //65535 キー値からキー コードを抽出するビット マスク
        public Key L { get; set; } //76 L キー
        public Key LaunchApplication1 { get; set; } //182 カスタム ホット キー 1
        public Key LaunchApplication2 { get; set; } //183 カスタム ホット キー 2
        public Key LaunchMail { get; set; } //180 メールの起動キー
        public Key LButton { get; set; } //1 マウスの左ボタン
        public Key LControlKey { get; set; } //162 左 Ctrl キー
        public Key Left { get; set; } //37 ←キー
        public Key LineFeed { get; set; } //10 ライン フィード キー
        public Key LMenu { get; set; } //164 左 Alt キー
        public Key LShiftKey { get; set; } //160 左の Shift キー
        public Key LWin { get; set; } //91 左 Windows ロゴ キー (Microsoft Natural Keyboard)
        public Key M { get; set; } //77 M キー
        public Key MButton { get; set; } //4 マウスの中央ボタン (3 ボタン マウスの場合)
        public Key MediaNextTrack { get; set; } //176 メディアの次のトラック キー
        public Key MediaPlayPause { get; set; } //179 メディアの再生/一時停止キー
        public Key MediaPreviousTrack { get; set; } //177 メディアの前のトラック キー
        public Key MediaStop { get; set; } //178 メディアの停止キー
        public Key Menu { get; set; } //18 Alt キー
        public Key Modifiers { get; set; } //-65536 キー値から修飾子を抽出するビット マスク
        public Key Multiply { get; set; } //106 乗算記号 (*) キー
        public Key N { get; set; } //78 N キー
        public Key Next { get; set; } //34 Page Down キー
        public Key NoName { get; set; } //252 将来使用するために予約されている定数
        public Key None { get; set; } //0 押されたキーがありません
        public Key NumLock { get; set; } //144 NUM LOCK キー
        public Key NumPad0 { get; set; } //96 0 キー (テンキー)
        public Key NumPad1 { get; set; } //97 1 キー (テンキー)
        public Key NumPad2 { get; set; } //98 2 キー (テンキー)
        public Key NumPad3 { get; set; } //99 3 キー (テンキー)
        public Key NumPad4 { get; set; } //100 4 キー (テンキー)
        public Key NumPad5 { get; set; } //101 5 キー (テンキー)
        public Key NumPad6 { get; set; } //102 6 キー (テンキー)
        public Key NumPad7 { get; set; } //103 7 キー (テンキー)
        public Key NumPad8 { get; set; } //104 8 キー (テンキー)
        public Key NumPad9 { get; set; } //105 9 キー (テンキー)
        public Key O { get; set; } //79 O キー
        public Key Oem1 { get; set; } //186 OEM 1 キー
        public Key Oem102 { get; set; } //226 OEM 102 キー
        public Key Oem2 { get; set; } //191 OEM 2 キー
        public Key Oem3 { get; set; } //192 OEM 3 キー
        public Key Oem4 { get; set; } //219 OEM 4 キー
        public Key Oem5 { get; set; } //220 OEM 5 キー
        public Key Oem6 { get; set; } //221 OEM 6 キー
        public Key Oem7 { get; set; } //222 OEM 7 キー
        public Key Oem8 { get; set; } //223 OEM 8 キー
        public Key OemBackslash { get; set; } //226 RT 102 キーのキーボード上の OEM 山かっこキーまたは円記号キー
        public Key OemClear { get; set; } //254 Clear キー
        public Key OemCloseBrackets { get; set; } //221 米国標準キーボード上の OEM 右角かっこキー
        public Key Oemcomma { get; set; } //188 国または地域別キーボード上の OEM コンマ キー
        public Key OemMinus { get; set; } //189 国または地域別キーボード上の OEM マイナス キー
        public Key OemOpenBrackets { get; set; } //219 米国標準キーボード上の OEM 左角かっこキー
        public Key OemPeriod { get; set; } //190 国または地域別キーボード上の OEM ピリオド キー
        public Key OemPipe { get; set; } //220 米国標準キーボード上の OEM パイプ キー
        public Key Oemplus { get; set; } //187 国または地域別キーボード上の OEM プラス キー
        public Key OemQuestion { get; set; } //191 米国標準キーボード上の OEM 疑問符キー
        public Key OemQuotes { get; set; } //222 米国標準キーボード上の OEM 一重/二重引用符キー
        public Key OemSemicolon { get; set; } //186 米国標準キーボード上の OEM セミコロン キー
        public Key Oemtilde { get; set; } //192 米国標準キーボード上の OEM チルダ キー
        public Key P { get; set; } //80 P キー
        public Key Pa1 { get; set; } //253 PA1 キー
        public Key Packet { get; set; } //231 Unicode 文字がキーストロークであるかのように渡されます Packet のキー値は、キーボード以外の入力手段に使用される 32 ビット仮想キー値の下位ワードです
        public Key PageDown { get; set; } //34 Page Down キー
        public Key PageUp { get; set; } //33 Page Up キー
        public Key Pause { get; set; } //19 Pause キー
        public Key Play { get; set; } //250 Play キー
        public Key Print { get; set; } //42 Print キー
        public Key PrintScreen { get; set; } //44 Print Screen キー
        public Key Prior { get; set; } //33 Page Up キー
        public Key ProcessKey { get; set; } //229 ProcessKey キー
        public Key Q { get; set; } //81 Q キー
        public Key R { get; set; } //82 R キー
        public Key RButton { get; set; } //2 マウスの右ボタン
        public Key RControlKey { get; set; } //163 右 Ctrl キー
        public Key Return { get; set; } //13 Return キー
        public Key Right { get; set; } //39 →キー
        public Key RMenu { get; set; } //165 右 Alt キー
        public Key RShiftKey { get; set; } //161 右の Shift キー
        public Key RWin { get; set; } //92 右 Windows ロゴ キー (Microsoft Natural Keyboard)
        public Key S { get; set; } //83 S キー
        public Key Scroll { get; set; } //145 ScrollLock キー
        public Key Select { get; set; } //41 Select キー
        public Key SelectMedia { get; set; } //181 メディアの選択キー
        public Key Separator { get; set; } //108 区切り記号キー
        public Key Shift { get; set; } //65536 Shift 修飾子キー
        public Key ShiftKey { get; set; } //16 Shift キー
        public Key Sleep { get; set; } //95 コンピューターのスリープ キー
        public Key Snapshot { get; set; } //44 Print Screen キー
        public Key Space { get; set; } //32 Space キー
        public Key Subtract { get; set; } //109 減算記号 (-) キー
        public Key T { get; set; } //84 T キー
        public Key Tab { get; set; } //9 Tab キー
        public Key U { get; set; } //85 U キー
        public Key Up { get; set; } //38 ↑キー
        public Key V { get; set; } //86 V キー
        public Key VolumeDown { get; set; } //174 音量下げるキー
        public Key VolumeMute { get; set; } //173 音量ミュート キー
        public Key VolumeUp { get; set; } //175 音量上げるキー
        public Key W { get; set; } //87 W キー
        public Key X { get; set; } //88 X キー
        public Key XButton1 { get; set; } //5 x マウスの 1 番目のボタン (5 ボタン マウスの場合)
        public Key XButton2 { get; set; } //6 x マウスの 2 番目のボタン (5 ボタン マウスの場合)
        public Key Y { get; set; } //89 Y キー
        public Key Z { get; set; }      //90 Z キー
        public Key Zoom { get; set; }   //251 Zoom キー


        /// コンストラクタ
        public Map(ViewModel vm) {
            _vm = vm;


            A = new Key(_vm);
            Add = new Key(_vm);
            Alt = new Key(_vm);
            Apps = new Key(_vm);
            Attn = new Key(_vm);
            B = new Key(_vm);
            Back = new Key(_vm);
            BrowserBack = new Key(_vm);
            BrowserFavorites = new Key(_vm);
            BrowserForward = new Key(_vm);
            BrowserHome = new Key(_vm);
            BrowserRefresh = new Key(_vm);
            BrowserSearch = new Key(_vm);
            BrowserStop = new Key(_vm);
            C = new Key(_vm);
            Cancel = new Key(_vm);
            Capital = new Key(_vm);
            CapsLock = new Key(_vm);
            Clear = new Key(_vm);
            Control = new Key(_vm);
            ControlKey = new Key(_vm);
            Crsel = new Key(_vm);
            D = new Key(_vm);
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
            Decimal = new Key(_vm);
            Delete = new Key(_vm);
            Divide = new Key(_vm);
            Down = new Key(_vm);
            E = new Key(_vm);
            End = new Key(_vm);
            Enter = new Key(_vm);
            EraseEof = new Key(_vm);
            Escape = new Key(_vm);
            Execute = new Key(_vm);
            Exsel = new Key(_vm);
            F = new Key(_vm);
            F1 = new Key(_vm);
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
            F2 = new Key(_vm);
            F20 = new Key(_vm);
            F21 = new Key(_vm);
            F22 = new Key(_vm);
            F23 = new Key(_vm);
            F24 = new Key(_vm);
            F3 = new Key(_vm);
            F4 = new Key(_vm);
            F5 = new Key(_vm);
            F6 = new Key(_vm);
            F7 = new Key(_vm);
            F8 = new Key(_vm);
            F9 = new Key(_vm);
            FinalMode = new Key(_vm);
            G = new Key(_vm);
            H = new Key(_vm);
            HanguelMode = new Key(_vm);
            HangulMode = new Key(_vm);
            HanjaMode = new Key(_vm);
            Help = new Key(_vm);
            Home = new Key(_vm);
            I = new Key(_vm);
            IMEAccept = new Key(_vm);
            IMEAceept = new Key(_vm);
            IMEConvert = new Key(_vm);
            IMEModeChange = new Key(_vm);
            IMENonconvert = new Key(_vm);
            Insert = new Key(_vm);
            J = new Key(_vm);
            JunjaMode = new Key(_vm);
            K = new Key(_vm);
            KanaMode = new Key(_vm);
            KanjiMode = new Key(_vm);
            KeyCode = new Key(_vm);
            L = new Key(_vm);
            LaunchApplication1 = new Key(_vm);
            LaunchApplication2 = new Key(_vm);
            LaunchMail = new Key(_vm);
            LButton = new Key(_vm);
            LControlKey = new Key(_vm);
            Left = new Key(_vm);
            LineFeed = new Key(_vm);
            LMenu = new Key(_vm);
            LShiftKey = new Key(_vm);
            LWin = new Key(_vm);
            M = new Key(_vm);
            MButton = new Key(_vm);
            MediaNextTrack = new Key(_vm);
            MediaPlayPause = new Key(_vm);
            MediaPreviousTrack = new Key(_vm);
            MediaStop = new Key(_vm);
            Menu = new Key(_vm);
            Modifiers = new Key(_vm);
            Multiply = new Key(_vm);
            N = new Key(_vm);
            Next = new Key(_vm);
            NoName = new Key(_vm);
            None = new Key(_vm);
            NumLock = new Key(_vm);
            NumPad0 = new Key(_vm);
            NumPad1 = new Key(_vm);
            NumPad2 = new Key(_vm);
            NumPad3 = new Key(_vm);
            NumPad4 = new Key(_vm);
            NumPad5 = new Key(_vm);
            NumPad6 = new Key(_vm);
            NumPad7 = new Key(_vm);
            NumPad8 = new Key(_vm);
            NumPad9 = new Key(_vm);
            O = new Key(_vm);
            Oem1 = new Key(_vm);
            Oem102 = new Key(_vm);
            Oem2 = new Key(_vm);
            Oem3 = new Key(_vm);
            Oem4 = new Key(_vm);
            Oem5 = new Key(_vm);
            Oem6 = new Key(_vm);
            Oem7 = new Key(_vm);
            Oem8 = new Key(_vm);
            OemBackslash = new Key(_vm);
            OemClear = new Key(_vm);
            OemCloseBrackets = new Key(_vm);
            Oemcomma = new Key(_vm);
            OemMinus = new Key(_vm);
            OemOpenBrackets = new Key(_vm);
            OemPeriod = new Key(_vm);
            OemPipe = new Key(_vm);
            Oemplus = new Key(_vm);
            OemQuestion = new Key(_vm);
            OemQuotes = new Key(_vm);
            OemSemicolon = new Key(_vm);
            Oemtilde = new Key(_vm);
            P = new Key(_vm);
            Pa1 = new Key(_vm);
            Packet = new Key(_vm);
            PageDown = new Key(_vm);
            PageUp = new Key(_vm);
            Pause = new Key(_vm);
            Play = new Key(_vm);
            Print = new Key(_vm);
            PrintScreen = new Key(_vm);
            Prior = new Key(_vm);
            ProcessKey = new Key(_vm);
            Q = new Key(_vm);
            R = new Key(_vm);
            RButton = new Key(_vm);
            RControlKey = new Key(_vm);
            Return = new Key(_vm);
            Right = new Key(_vm);
            RMenu = new Key(_vm);
            RShiftKey = new Key(_vm);
            RWin = new Key(_vm);
            S = new Key(_vm);
            Scroll = new Key(_vm);
            Select = new Key(_vm);
            SelectMedia = new Key(_vm);
            Separator = new Key(_vm);
            Shift = new Key(_vm);
            ShiftKey = new Key(_vm);
            Sleep = new Key(_vm);
            Snapshot = new Key(_vm);
            Space = new Key(_vm);
            Subtract = new Key(_vm);
            T = new Key(_vm);
            Tab = new Key(_vm);
            U = new Key(_vm);
            Up = new Key(_vm);
            V = new Key(_vm);
            VolumeDown = new Key(_vm);
            VolumeMute = new Key(_vm);
            VolumeUp = new Key(_vm);
            W = new Key(_vm);
            X = new Key(_vm);
            XButton1 = new Key(_vm);
            XButton2 = new Key(_vm);
            Y = new Key(_vm);
            Z = new Key(_vm);
            Zoom = new Key(_vm);

        }

        // Indexer
        // **************************************************
        public Key this[string propertyName]
        {
            get
            {
                return (Key)typeof(Map).GetProperty(propertyName).GetValue(this);
            }
            set
            {
                typeof(Map).GetProperty(propertyName).SetValue(this, value);
                MapUpdateEventHandler(this, new MapUpdateEventArgs()
                {
                    mapName = Name,
                    propertyName = propertyName
                });
            }
        }

        // Event
        // **************************************************
        internal event EventHandler<MapUpdateEventArgs> MapUpdateEventHandler;

        // SubClass
        // **************************************************
        public class MapUpdateEventArgs
        {
            public string mapName;
            public string propertyName;
        }

    }
}
