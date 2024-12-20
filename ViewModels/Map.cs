using System;

namespace QwertyLauncher
{
    public class Map
    {
        // Properties
        // **************************************************
        public string Name { get; set; }
        private ViewModel _vm;

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

        public Key D1 { get; set; }
        public Key D2 { get; set; }
        public Key D3 { get; set; }
        public Key D4 { get; set; }
        public Key D5 { get; set; }
        public Key D6 { get; set; }
        public Key D7 { get; set; }
        public Key D8 { get; set; }
        public Key D9 { get; set; }
        public Key D0 { get; set; }
        public Key OemMinus { get; set; }
        public Key Oem7 { get; set; }
        public Key Oem5 { get; set; }
        public Key Back { get; set; }

        public Key Q { get; set; }
        public Key W { get; set; }
        public Key E { get; set; }
        public Key R { get; set; }
        public Key T { get; set; }
        public Key Y { get; set; }
        public Key U { get; set; }
        public Key I { get; set; }
        public Key O { get; set; }
        public Key P { get; set; }
        public Key Oemtilde { get; set; }
        public Key OemOpenBrackets { get; set; }

        public Key A { get; set; }
        public Key S { get; set; }
        public Key D { get; set; }
        public Key F { get; set; }
        public Key G { get; set; }
        public Key H { get; set; }
        public Key J { get; set; }
        public Key K { get; set; }
        public Key L { get; set; }
        public Key Oemplus { get; set; }
        public Key Oem1 { get; set; }
        public Key Oem6 { get; set; }

        public Key Z { get; set; }
        public Key X { get; set; }
        public Key C { get; set; }
        public Key V { get; set; }
        public Key B { get; set; }
        public Key N { get; set; }
        public Key M { get; set; }
        public Key Oemcomma { get; set; }
        public Key OemPeriod { get; set; }
        public Key OemQuestion { get; set; }
        public Key OemBackslash { get; set; }

        public Key NumPad0 { get; set; }
        public Key NumPad1 { get; set; }
        public Key NumPad2 { get; set; }
        public Key NumPad3 { get; set; }
        public Key NumPad4 { get; set; }
        public Key NumPad5 { get; set; }
        public Key NumPad6 { get; set; }
        public Key NumPad7 { get; set; }
        public Key NumPad8 { get; set; }
        public Key NumPad9 { get; set; }
        public Key Decimal { get; set; }

        public Map(ViewModel vm) {
            _vm = vm;
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
            D1 = new Key(_vm);
            D2 = new Key(_vm);
            D3 = new Key(_vm);
            D4 = new Key(_vm);
            D5 = new Key(_vm);
            D6 = new Key(_vm);
            D7 = new Key(_vm);
            D8 = new Key(_vm);
            D9 = new Key(_vm);
            D0 = new Key(_vm);
            OemMinus = new Key(_vm);
            Oem7 = new Key(_vm);
            Oem5 = new Key(_vm);
            Back = new Key(_vm);
            Q = new Key(_vm);
            W = new Key(_vm);
            E = new Key(_vm);
            R = new Key(_vm);
            T = new Key(_vm);
            Y = new Key(_vm);
            U = new Key(_vm);
            I = new Key(_vm);
            O = new Key(_vm);
            P = new Key(_vm);
            Oemtilde = new Key(_vm);
            OemOpenBrackets = new Key(_vm);
            A = new Key(_vm);
            S = new Key(_vm);
            D = new Key(_vm);
            F = new Key(_vm);
            G = new Key(_vm);
            H = new Key(_vm);
            J = new Key(_vm);
            K = new Key(_vm);
            L = new Key(_vm);
            Oemplus = new Key(_vm);
            Oem1 = new Key(_vm);
            Oem6 = new Key(_vm);
            Z = new Key(_vm);
            X = new Key(_vm);
            C = new Key(_vm);
            V = new Key(_vm);
            B = new Key(_vm);
            N = new Key(_vm);
            M = new Key(_vm);
            Oemcomma = new Key(_vm);
            OemPeriod = new Key(_vm);
            OemQuestion = new Key(_vm);
            OemBackslash = new Key(_vm);
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
            Decimal = new Key(_vm);


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
