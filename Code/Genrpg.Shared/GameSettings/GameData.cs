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
    [MessagePackObject]
    public class GameData
    {
        public const int IdBlockSize = 10000;

        [Key(0)] public DateTime PrevSaveTime { get; set; }  = DateTime.MinValue;
        [Key(1)] public DateTime CurrSaveTime { get; set; } = DateTime.UtcNow;

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
        public T GetGameData<T>(IFilteredObject obj) where T : IGameSettings
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
            IGameSettings currentObject = _allData.FirstOrDefault(x => x.Id == t.Id && x.GetType() == t.GetType());
            if (currentObject != null)
            {
                _allData.Remove(currentObject);
            }
                                       
            _allData.Add(t);
        }

        public void AddData(List<IGameSettings> settingsList)
        {
            foreach (IGameSettings settings in settingsList)
            {
                settings.AddTo(this);
            }
            SetupDataDict(true);
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

        public string GetDataObjectName(string typeName, IFilteredObject obj)
        {
            return obj?.GetGameDataName(typeName) ?? GameDataConstants.DefaultFilename;
            
        }
    }
}
