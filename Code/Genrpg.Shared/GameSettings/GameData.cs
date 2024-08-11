using MessagePack;

using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.PlayerData;

namespace Genrpg.Shared.GameSettings
{
    public interface IGameData : IInjectable
    {
        DateTime PrevSaveTime { get; set; }
        DateTime CurrSaveTime { get; set; }
        List<ITopLevelSettings> AllSettings();
        void ClearIndex();
        void SetupDataDict(bool force);
        T Get<T>(IFilteredObject obj) where T : IGameSettings;
        void Set<T>(T t) where T : ITopLevelSettings;
        void AddData(List<ITopLevelSettings> settingsList);
        string SettingObjectName(string typeName, IFilteredObject obj);
        void CopyFrom(IGameData data);
    }

    [MessagePackObject]
    public class GameData : IGameData
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

        public void CopyFrom(IGameData gameData)
        {
            _allData = gameData.AllSettings();
            ClearIndex();
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
        public virtual T Get<T>(IFilteredObject obj) where T : IGameSettings
        {
            SetupDataDict(false);

            string dataName = SettingObjectName(typeof(T).Name, obj);

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

        public string SettingObjectName(string settingName, IFilteredObject obj)
        {
            if (obj == null || obj.DataOverrides == null || obj.DataOverrides.Items == null)
            {
                return GameDataConstants.DefaultFilename;
            }
            PlayerSettingsOverrideItem item = obj.DataOverrides.Items.FirstOrDefault(x => x.SettingId == settingName);
            return item?.DocId ?? GameDataConstants.DefaultFilename;
        }
    }
}
