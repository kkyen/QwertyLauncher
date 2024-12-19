using System;
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
        // コンストラクタ
        // **************************************************
        public Config()
        {
            InitializeDefaultConfig();
        }

        public Config(string filePath)
        {
            _filepath = filePath;
            if (File.Exists(_filepath))
            {
                LoadConfigFromFile();
            }
            else
            {
                InitializeDefaultConfig();
            }
        }

        // プロパティ
        // **************************************************
        private string _filepath;

        private string _theme = "auto";
        public string Theme
        {
            get => _theme;
            set => SaveChangedIfSet(ref _theme, value);
        }

        private string[] _activateKeys = { "LWin", "NumLock" };
        public string[] ActivateKeys
        {
            get => _activateKeys;
            set => SaveChangedIfSet(ref _activateKeys, value);
        }

        private bool _activateWithTaskbarDoubleClick = true;
        public bool ActivateWithTaskbarDoubleClick
        {
            get => _activateWithTaskbarDoubleClick;
            set => SaveChangedIfSet(ref _activateWithTaskbarDoubleClick, value);
        }

        private bool _showQwerty = true;
        public bool ShowQwerty
        {
            get => _showQwerty;
            set => SaveChangedIfSet(ref _showQwerty, value);
        }

        private bool _showFunction = false;
        public bool ShowFunction
        {
            get => _showFunction;
            set => SaveChangedIfSet(ref _showFunction, value);
        }

        private bool _showNumPad = false;
        public bool ShowNumPad
        {
            get => _showNumPad;
            set => SaveChangedIfSet(ref _showNumPad, value);
        }

        private int _doubleClickSpeed = 300;
        public int DoubleClickSpeed
        {
            get => _doubleClickSpeed;
            set => SaveChangedIfSet(ref _doubleClickSpeed, value);
        }

        private bool _advancedMouseRecording = false;
        public bool AdvancedMouseRecording
        {
            get => _advancedMouseRecording;
            set => SaveChangedIfSet(ref _advancedMouseRecording, value);
        }

        private bool _downloadFavicon = true;
        public bool DownloadFavicon
        {
            get => _downloadFavicon;
            set => SaveChangedIfSet(ref _downloadFavicon, value);
        }

        private bool _autoUpdate = true;
        public bool AutoUpdate
        {
            get => _autoUpdate;
            set => SaveChangedIfSet(ref _autoUpdate, value);
        }

        public Dictionary<string, string> CustomTheme { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, Map> Maps { get; set; } = new Dictionary<string, Map>();

        // メソッド
        // **************************************************
        public void Save()
        {
            if (_filepath != null)
            {
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    WriteIndented = true
                };

                string jsonStr = JsonSerializer.Serialize(this, options);
                App.WatchConfig.EnableRaisingEvents = false;
                File.WriteAllText(_filepath, jsonStr);
                App.WatchConfig.EnableRaisingEvents = true;
            }
        }

        private void LoadConfigFromFile()
        {
            string jsonStr;
            using (var sr = new StreamReader(_filepath, Encoding.UTF8))
            {
                jsonStr = sr.ReadToEnd();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                MaxDepth = 64
            };

            var conf = JsonSerializer.Deserialize<Config>(jsonStr, options);

            _theme = conf.Theme;
            _activateKeys = conf.ActivateKeys;
            _activateWithTaskbarDoubleClick = conf.ActivateWithTaskbarDoubleClick;
            _showQwerty = conf.ShowQwerty;
            _showFunction = conf.ShowFunction;
            _showNumPad = conf.ShowNumPad;
            _doubleClickSpeed = conf.DoubleClickSpeed;
            _advancedMouseRecording = conf.AdvancedMouseRecording;
            _downloadFavicon = conf.DownloadFavicon;
            Maps = conf.Maps;
            CustomTheme = conf.CustomTheme;
        }

        private void InitializeDefaultConfig()
        {
            Maps.Add("Root", new Map());

            var explorerKey = new Key
                {
                    { "name", "Explorer" },
                    { "path", "explorer.exe" }
                };
            Maps["Root"].Add("E", explorerKey);

            var notepadKey = new Key
                {
                    { "name", "Notepad" },
                    { "path", "notepad.exe" }
                };
            Maps["Root"].Add("N", notepadKey);
        }

        private protected bool SaveChangedIfSet<TResult>(ref TResult source, TResult value)
        {
            if (EqualityComparer<TResult>.Default.Equals(source, value))
                return false;
            source = value;
            Save();
            return true;
        }

        // サブクラス
        // **************************************************
        public class Map : Dictionary<string, Key> { }
        public class Key : Dictionary<string, object> { }
    }
}