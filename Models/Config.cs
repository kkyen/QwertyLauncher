using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace QwertyLauncher.Models
{

    public class Config
    {
        // Constructer
        // **************************************************
        public Config()
        {
            Maps.Add("default", new Map());

            Key key = new Key
            {
                { "name", "Explorer" },
                { "path", "explorer.exe" }
            };
            Maps["default"].Add("E", key);
            key = new Key
            {
                { "name", "Notepad" },
                { "path", "notepad.exe" }
            };
            Maps["default"].Add("N", key);

        }

        public Config(string FilePath)
        {
            _filepath = FilePath;
            string jsonstr;
            Config conf = new Config();
            if (File.Exists(_filepath)){

                using (StreamReader sr = new StreamReader(_filepath, Encoding.UTF8))
                {
                    jsonstr = sr.ReadToEnd();
                }
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                    MaxDepth = 64
                };
                conf = JsonSerializer.Deserialize<Config>(jsonstr, options);
            }

            _Theme = conf.Theme;
            _ActivateKeys = conf.ActivateKeys;
            _ActivateWithTaskbarDoubleClick = conf.ActivateWithTaskbarDoubleClick;
            _ShowQwerty = conf.ShowQwerty;
            _ShowFunction = conf.ShowFunction;
            _ShowNumPad = conf.ShowNumPad;
            _DoubleClickSpeed = conf.DoubleClickSpeed;
            _AdvancedMouseRecording = conf.AdvancedMouseRecording;
            Maps = conf.Maps;
        }

        // Properties
        // **************************************************
        private string _filepath;

        private string _Theme = "auto";
        public string Theme
        {
            get => _Theme;
            set { SaveChangedIfSet(ref _Theme, value); }
        }

        private string[] _ActivateKeys = {"LWin", "NumLock"};
        public string[] ActivateKeys
        {
            get => _ActivateKeys;
            set { SaveChangedIfSet(ref _ActivateKeys, value); }
        }

        private bool _ActivateWithTaskbarDoubleClick = true;
        public bool ActivateWithTaskbarDoubleClick
        {
            get => _ActivateWithTaskbarDoubleClick;
            set { SaveChangedIfSet(ref _ActivateWithTaskbarDoubleClick, value); }
        }


        private bool _ShowQwerty = true;
        public bool ShowQwerty
        {
            get => _ShowQwerty;
            set { SaveChangedIfSet(ref _ShowQwerty, value); }
        }

        private bool _ShowFunction = false;
        public bool ShowFunction
        {
            get => _ShowFunction;
            set { SaveChangedIfSet(ref _ShowFunction, value); }
        }

        private bool _ShowNumPad = false;
        public bool ShowNumPad
        {
            get { return _ShowNumPad; }
            set { SaveChangedIfSet(ref _ShowNumPad, value); }
        }

        private int _DoubleClickSpeed = 300;
        public int DoubleClickSpeed
        {
            get { return _DoubleClickSpeed; }
            set { SaveChangedIfSet(ref _DoubleClickSpeed, value); }
        }
        private bool _AdvancedMouseRecording = false;
        public bool AdvancedMouseRecording
        {
            get { return _AdvancedMouseRecording; }
            set { SaveChangedIfSet(ref _AdvancedMouseRecording, value); }
        }

        public Dictionary<string, Map> Maps { get; set; } = new Dictionary<string, Map>();


        // Methods
        // **************************************************
        public void Save()
        {
            if (_filepath != null)
            {
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    WriteIndented = true
                };

                string jsonstr = JsonSerializer.Serialize(this, options);
                //Debug.WriteLine(jsonstr);
                App.WatchConfig.EnableRaisingEvents = false;
                File.WriteAllText(_filepath, jsonstr);
                App.WatchConfig.EnableRaisingEvents = true;
            }
        }

        private protected bool SaveChangedIfSet<TResult>(ref TResult source, TResult value)
        {
            if (EqualityComparer<TResult>.Default.Equals(source, value))
                return false;
            source = value;
            Save();
            return true;
        }

        // SubClass
        // **************************************************
        public class Map : Dictionary<string, Key> { }
        public class Key : Dictionary<string, object> { }
    }
}