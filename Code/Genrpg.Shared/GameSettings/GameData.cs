using MessagePack;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using Genrpg.Shared.Levels.Entities;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.AI.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Input.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Factions.Entities;
using Genrpg.Shared.Names.Entities;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.Crafting.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.DataStores.Categories;
using Newtonsoft.Json;
using Genrpg.Shared.GameSettings.Interfaces;

namespace Genrpg.Shared.GameSettings
{
    public class GameData
    {
        public const int IdBlockSize = 10000;

        public List<IGameSettingsContainer> GetContainers()
        {
            return _dataContainers;
        }

        private List<IGameSettingsContainer> _dataContainers { get; set; } = new List<IGameSettingsContainer>();

        public GameData()
        {
        }

        private Dictionary<Type, BaseGameData> _dataDict = null;
        public T GetGameData<T>() where T : BaseGameData
        {
            if (_dataDict == null)
            {
                Dictionary<Type, BaseGameData> tempDict = new Dictionary<Type, BaseGameData>();
                foreach (IGameSettingsContainer cont in _dataContainers)
                {
                    BaseGameData data = cont.GetData();
                    tempDict[data.GetType()] = data;
                }
                _dataDict = tempDict;
            }
            if (_dataDict.TryGetValue(typeof(T), out BaseGameData value))
            {
                return (T)value;
            }

            return null;
        }

        public void Set<T>(T t) where T : BaseGameData, new()
        {
            _dataContainers.Add(new GameSettingsContainer<T>() { DataObject = t });
        }

        public List<IIdName> GetList(string typeName)
        {
            foreach (IGameSettingsContainer cont in _dataContainers)
            {
                var data = cont.GetData();
                List<IIdName> vals = data.GetList(typeName);
                if (vals != null && vals.Count > 0)
                {
                    return vals;
                }
            }
            return new List<IIdName>();
        }


        public List<T> GetList<T>()
        {
            foreach (IGameSettingsContainer cont in _dataContainers)
            {
                var data = cont.GetData();
                List<T> vals = data.GetList<T>();
                if (vals != null && vals.Count > 0)
                {
                    return vals;
                }
            }

            return new List<T>();
        }
    }
}
