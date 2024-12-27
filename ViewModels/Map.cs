using System;
using System.Collections.Generic;
using System.Reflection.Emit;

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

        public Dictionary<string, Mod> Mods = new Dictionary<string, Mod>();


        /// コンストラクタ
        public Map(ViewModel vm) {
            _vm = vm;
        }

        // Indexer
        // **************************************************
        public Mod this[string propertyName]
        {
            get
            {
                return Mods[propertyName];
            }
        }

        public void ModUpdate(object e, Mod.ModUpdateEventArgs args)
        {
            MapUpdateEventHandler(this, new MapUpdateEventArgs()
            {
                mapName = Name,
                modName = args.modName,
                keyName = args.keyName

            });
        }

        // Event
        // **************************************************
        internal event EventHandler<MapUpdateEventArgs> MapUpdateEventHandler;

        // SubClass
        // **************************************************
        public class MapUpdateEventArgs
        {
            public string mapName;
            public string modName;
            public string keyName;
        }

    }
}
