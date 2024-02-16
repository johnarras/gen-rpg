using MessagePack;

using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.DataStores.GameSettings;

namespace Genrpg.Shared.GameSettings
{
    [MessagePackObject]
    public class GameData
    {
        public const int IdBlockSize = 10000;

        [Key(0)] public DateTime PrevSaveTime { get; set; }  = DateTime.MinValue;
        [Key(1)] public DateTime CurrSaveTime { get; set; } = DateTime.UtcNow;

        private List<ITopLevelSettings> _allData { get; set; } = new List<ITopLevelSettings>();

        public List<ITopLevelSettings> AllSettings()
        {
            return _allData;
        }

        public GameData()
        {
        }

        public void ClearIndex()
        {
            foreach (IGameSettings settings in _allData)
            {
                settings.ClearIndex();
            }
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
        public T Get<T>(IFilteredObject obj) where T : IGameSettings
        {
            SetupDataDict(false);

            string dataName = DataObjectName(typeof(T).Name, obj);

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

        public void Set<T>(T t) where T : ITopLevelSettings
        {
            if (t is IChildSettings childSettings)
            {
                return;
            }

            ITopLevelSettings currentObject = _allData.FirstOrDefault(x => x.Id == t.Id && x.GetType() == t.GetType());
            if (currentObject != null)
            {
                _allData.Remove(currentObject);
            }
                                       
            _allData.Add(t);
        }

        public void AddData(List<ITopLevelSettings> settingsList)
        {
            foreach (ITopLevelSettings settings in settingsList)
            {
                settings.AddTo(this);
            }
            SetupDataDict(true);
        }

        public string DataObjectName(string typeName, IFilteredObject obj)
        {
            return obj?.GetName(typeName) ?? GameDataConstants.DefaultFilename;
            
        }
    }
}
