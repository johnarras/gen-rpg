using MessagePack;

using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;

namespace Genrpg.Shared.GameSettings
{
    public class GameData
    {
        public const int IdBlockSize = 10000;

        private List<IGameSettings> _allData { get; set; } = new List<IGameSettings>();

        public List<IGameSettings> GetAllData()
        {
            return _allData;
        }

        public GameData()
        {
        }

        public void SetupDataDict(bool force)
        {
            if (_dataDict == null || force)
            {
                Dictionary<Type, Dictionary<string, IGameSettings>> tempDict = new Dictionary<Type, Dictionary<string, IGameSettings>>();
                foreach (IGameSettings data in _allData)
                {
                    if (!tempDict.TryGetValue(data.GetType(), out Dictionary<string, IGameSettings> dataDict))
                    {
                        dataDict = new Dictionary<string, IGameSettings>();
                        tempDict.Add(data.GetType(), dataDict);
                    }

                    dataDict[data.Id] = data;
                }
                _dataDict = tempDict;
            }
        }

        private Dictionary<Type, Dictionary<string,IGameSettings>> _dataDict = null!;
        public T GetGameData<T>(IFilteredObject? obj) where T : IGameSettings
        {
            SetupDataDict(false);

            string dataName = GetDataObjectName(typeof(T).Name, obj);

            if (_dataDict.TryGetValue(typeof(T), out Dictionary<string, IGameSettings> typeDict))
            {
                if (typeDict.TryGetValue(dataName, out IGameSettings data))
                {
                    return (T)data;
                }

                if (typeDict.TryGetValue(GameDataConstants.DefaultFilename, out IGameSettings defaultData))
                {
                    return (T)defaultData;
                }

                return (T)typeDict.Values.FirstOrDefault();
            }

            return default(T)!;
        }

        public void Set<T>(T t) where T : IGameSettings
        {
            _allData.Add(t);
        }

        public List<IIdName> GetList(string typeName)
        {
            foreach (BaseGameSettings data in _allData)
            {
                if (data.Id != GameDataConstants.DefaultFilename)
                {
                    continue;
                }
                List<IIdName> vals = data.GetList(typeName);
                if (vals != null && vals.Count > 0)
                {
                    return vals;
                }
            }
            return new List<IIdName>();
        }

        public string GetDataObjectName(string typeName, IFilteredObject? obj)
        {
            return obj?.GetGameDataName(typeName) ?? GameDataConstants.DefaultFilename;
            
        }
    }
}
