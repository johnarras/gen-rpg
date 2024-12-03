using Genrpg.ServerShared.Crypto.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.GameSettings.Settings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.PlayerFiltering.Utils;
using Genrpg.Shared.Settings.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Versions.Settings;
using Genrpg.Shared.Website.Messages.Login;
using Genrpg.Shared.Website.Messages.RefreshGameSettings;
using Microsoft.Extensions.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.GameSettings.Services
{
    public class GameDataService<D> : IGameDataService where D : GameData, new()
    {
        SetupDictionaryContainer<Type, IGameSettingsLoader> _loaderObjects = new SetupDictionaryContainer<Type, IGameSettingsLoader>();    
        SetupDictionaryContainer<Type, IGameSettingsMapper> _mapperObjects = new SetupDictionaryContainer<Type, IGameSettingsMapper>();

        protected IRepositoryService _repoService = null;
        private IGameData _gameData = null;
        private ICryptoService _cryptoService = null;

        public List<IGameSettingsLoader> GetAllLoaders()
        {
            return _loaderObjects.GetDict().Values.OrderBy(x => x.GetType().Name).ToList();
        }

        public virtual List<string> GetEditorIgnoreFields()
        {
            return new List<string>() { "_lookup", "DocVersion" };
        }

        public virtual async Task<IGameData> LoadGameData(bool createMissingGameData)
        {
            GameData gameData = new GameData();

            List<Task<List<ITopLevelSettings>>> allTasks = new List<Task<List<ITopLevelSettings>>>();

            foreach (IGameSettingsLoader loader in _loaderObjects.GetDict().Values)
            {
                allTasks.Add(loader.LoadAll(_repoService, createMissingGameData));
            }

            List<ITopLevelSettings>[] allSettings = await Task.WhenAll(allTasks.ToArray());

            foreach (List<ITopLevelSettings> settingsList in allSettings)
            {
                foreach (IGameSettings settings in settingsList)
                {
                    settings.AddTo(gameData);
                }
            }
            gameData.SetupDataDict(true);

            gameData.CurrSaveTime = gameData.Get<VersionSettings>(null).GameDataSaveTime;

            _gameData.CopyFrom(gameData);

            return gameData;
        }

        public virtual async Task<bool> SaveGameData(IGameData data, IRepositoryService repoSystem)
        {
            D tdata = data as D;
            if (tdata == null)
            {
                return false;
            }

            foreach (IGameSettings baseGameData in data.AllSettings())
            {
                await repoSystem.Save(baseGameData);
            }
            return true;
        }

        public bool SetGameDataOverrides(IFilteredObject ch, bool forceRefresh)
        {

            if (ch == null || ch.DataOverrides == null)
            {
                return true;
            }

            VersionSettings versionSettings = _gameData.Get<VersionSettings>(null);

            DataOverrideSettings dataOverrideSettings = _gameData.Get<DataOverrideSettings>(null);

            SettingsNameSettings settingsNameSettings = _gameData.Get<SettingsNameSettings>(null);

            if (dataOverrideSettings.NextUpdateTime <= DateTime.UtcNow)
            {
                dataOverrideSettings.SetPrevNextUpdateTimes();
            }

            // If we are not force refreshing, don't always update the settings
            // If the game data wasn't saved and the player's LastTimeSet is after the PrevUpdateTime
            // (most recent override update) and it's before the NextUpdateTime then it means the
            // player has the most recent data and the next data hasn't changed, so don't make 
            // any changes.

            if (!forceRefresh &&
                versionSettings.GameDataSaveTime == ch.DataOverrides.GameDataSaveTime &&
                ch.DataOverrides.LastTimeSet >= dataOverrideSettings.PrevUpdateTime &&
                ch.DataOverrides.LastTimeSet < dataOverrideSettings.NextUpdateTime)
            {
                return false;
            }

            List<DataOverrideItemPriority> priorityOverrides = new List<DataOverrideItemPriority>();

            List<DataOverrideGroup> acceptableGroups = new List<DataOverrideGroup>();

            // dataOverrideSettings.GetData() is ordered the DataOverrideSettingsLoader
            foreach (DataOverrideGroup overrideGroup in dataOverrideSettings.GetData())
            {
                if (AcceptedByFilter(ch, overrideGroup))
                {
                    // Each group.Items is ordered on load by SettingsId then by DocId
                    foreach (DataOverrideItem groupItem in overrideGroup.Items)
                    {
                        if (groupItem.SettingsNameId < 1 ||
                            string.IsNullOrEmpty(groupItem.DocId) ||
                            !groupItem.Enabled ||
                            groupItem.DocId == GameDataConstants.DefaultFilename)
                        {
                            continue;
                        }

                        DataOverrideItemPriority overrideItem = priorityOverrides.FirstOrDefault(x => x.SettingsNameId == groupItem.SettingsNameId);

                        if (overrideItem == null)
                        {
                            overrideItem = new DataOverrideItemPriority { SettingsNameId = groupItem.SettingsNameId };
                            priorityOverrides.Add(overrideItem);
                            overrideItem.DocId = groupItem.DocId;
                            overrideItem.Priority = overrideGroup.Priority;
                        }
                        else if (overrideGroup.Priority > overrideItem.Priority)
                        {
                            overrideItem.Priority = overrideGroup.Priority;
                            overrideItem.DocId = groupItem.DocId;
                        }
                    }
                }
            }

            ch.DataOverrides.GameDataSaveTime = versionSettings.GameDataSaveTime;
            ch.DataOverrides.LastTimeSet = DateTime.UtcNow;

            ch.DataOverrides.Items = new List<PlayerSettingsOverrideItem>();

            foreach (DataOverrideItemPriority priority in priorityOverrides)
            {
                ch.DataOverrides.Items.Add(new PlayerSettingsOverrideItem()
                {
                    SettingId = settingsNameSettings.Get(priority.SettingsNameId).Name,
                    DocId = priority.DocId,
                });
            }

            ch.DataOverrides.Items = ch.DataOverrides.Items.OrderBy(x => x.SettingId).ToList();

            // This should be deterministic across machines because the player has a set
            // of overrides that should be the same for anyone who's in the same bucket
            // and then the game data save time and the prev update time (last time the
            // overrides changed) will be the same.
            string fullString = SerializationUtils.Serialize(ch.DataOverrides.Items) +
                versionSettings.GameDataSaveTime.Ticks.ToString() + "." +
                dataOverrideSettings.PrevUpdateTime.Ticks.ToString();

            ch.DataOverrides.Hash = _cryptoService.QuickHash(fullString);

            if (ch is CoreCharacter coreChar)
            {
                _repoService.QueueSave(coreChar);
            }
            else if (ch is ICoreCharacter icoreChar)
            { 
                CoreCharacter coreCharCopy = new CoreCharacter();
                CharacterUtils.CopyDataFromTo(icoreChar, coreCharCopy);
                _repoService.QueueSave(coreCharCopy);
            }

            return true;
        }

        public List<ITopLevelSettings> MapToApi(IFilteredObject obj, List<ITopLevelSettings> startSettings)
        {
            List<ITopLevelSettings> retval = new List<ITopLevelSettings>();

            Version clientVersion = new Version(obj.ClientVersion);

            foreach (ITopLevelSettings settings in startSettings)
            {
                if (_mapperObjects.TryGetValue(settings.GetType(), out IGameSettingsMapper mapper))
                {
                    if (clientVersion < mapper.MinClientVersion)
                    {
                        continue;
                    }

                    retval.Add(mapper.MapToApi(settings));
                }
                else
                {
                    retval.Add(settings);
                }
            }
            return retval;
        }

        public List<ITopLevelSettings> GetClientGameData(IFilteredObject fobj, bool sendAllDefault, List<ClientCachedGameSettings> clientCache = null)
        {

            List<ITopLevelSettings> retval = new List<ITopLevelSettings>();
            SetGameDataOverrides(fobj, true);

            List<ITopLevelSettings> allData = _gameData.AllSettings();

            Dictionary<Type,IGameSettingsMapper> mapperDict = _mapperObjects.GetDict();

            foreach (Type t in mapperDict.Keys)
            {
                IGameSettingsMapper mapper = mapperDict[t];

                if (!mapper.SendToClient())
                {
                    continue;
                }

                string docName = GameDataConstants.DefaultFilename;

                if (!sendAllDefault)
                {
                    docName = _gameData.SettingObjectName(t.Name, fobj);
                    if (docName == GameDataConstants.DefaultFilename)
                    {
                        continue;
                    }
                }

                ITopLevelSettings currData = allData.FirstOrDefault(x =>
                x.GetType().Name == t.Name &&
                x.Id == docName);

                if (clientCache != null && docName == GameDataConstants.DefaultFilename &&
                    currData != null && currData is IUpdateData updateData)
                {
                    ClientCachedGameSettings clientCachedItem = clientCache.FirstOrDefault(x => x.TypeName == mapper.GetKey().Name);

                    if (clientCachedItem != null && clientCachedItem.ClientSaveTime >= updateData.UpdateTime)
                    {
                        continue;
                    }

                }
                retval.Add(mapper.MapToApi(currData));
            }
            return retval;
        }

        public bool AcceptedByFilter(IFilteredObject obj, IPlayerFilter filter)
        {
            if (!PlayerFilterUtils.IsActive(filter))
            {
                return false;
            }

            if (filter.AllowedPlayers.Any(x=>x.PlayerId == obj.Id))
            {
                return true;
            }

            if (filter.MinLevel > 0 && filter.MaxLevel > 0 &&
                (filter.MinLevel > obj.Level || filter.MaxLevel < obj.Level))
            {
                return false;
            }

            if (filter.TotalModSize > 0 && filter.MaxAcceptableModValue > 0)
            {
                long idHash = StrUtils.GetIdHash(filter.IdKey + obj.Id);

                if (idHash % filter.TotalModSize >= filter.MaxAcceptableModValue)
                {
                    return false;
                }
            }

            if (filter.MaxUserDaysSinceInstall > 0 && (DateTime.UtcNow - obj.CreationDate).Days > filter.MaxUserDaysSinceInstall)
            {
                return false;
            }

            if (filter.MinUserDaysSinceInstall > 0 && (DateTime.UtcNow - obj.CreationDate).Days < filter.MinUserDaysSinceInstall)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(filter.MinClientVersion) || !string.IsNullOrEmpty(filter.MaxClientVersion))
            {
                if (string.IsNullOrEmpty(obj.ClientVersion))
                {
                    return false;
                }
                Version clientVersion = new Version(obj.ClientVersion);
                if (!string.IsNullOrEmpty(filter.MinClientVersion))
                {
                    Version minVersion = new Version(filter.MinClientVersion);  

                    if (clientVersion < minVersion)
                    {
                        return false;
                    }
                }
                if (!string.IsNullOrWhiteSpace(filter.MaxClientVersion))
                {
                    Version maxVersion = new Version(filter.MaxClientVersion);
                    if (clientVersion > maxVersion)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public async Task ReloadGameData()
        {
            IGameData newData = await LoadGameData(true);

            newData.PrevSaveTime = _gameData.CurrSaveTime;
        }

        public RefreshGameSettingsResult GetNewGameDataUpdates(IFilteredObject ch, bool forceRefresh)
        {
            if (!SetGameDataOverrides(ch, forceRefresh))
            {
                return null;
            }

            RefreshGameSettingsResult result = new RefreshGameSettingsResult();

            List<ITopLevelSettings> newSettings = new List<ITopLevelSettings>();

            // Always load this on client commands/always have in mapserver.

            List<PlayerSettingsOverrideItem> oldPlayerOverrides = new List<PlayerSettingsOverrideItem>(ch.DataOverrides.Items);

            GameDataOverrideList overrideList = ch.DataOverrides;

            List<ITopLevelSettings> allSettings = _gameData.AllSettings();

            foreach (ITopLevelSettings settings in allSettings)
            {
                BaseGameSettings baseSettings = settings as BaseGameSettings;

                if (baseSettings.Id == GameDataConstants.DefaultFilename)
                {
                    // If it's default data saved after the current save time, then send it to the client.
                    if (baseSettings.UpdateTime >= _gameData.CurrSaveTime)
                    {
                        newSettings.Add(settings);
                    }
                }
                else // Send all A/B test data to the client
                {
                    PlayerSettingsOverrideItem overrideItem = overrideList.Items.FirstOrDefault(x => x.SettingId == settings.GetType().Name &&
                    x.DocId == settings.Id);

                    if (overrideItem != null)
                    {
                        PlayerSettingsOverrideItem prevItem = oldPlayerOverrides.FirstOrDefault(x => x.SettingId == settings.GetType().Name);

                        if (prevItem == null || baseSettings.UpdateTime >= _gameData.CurrSaveTime ||
                            prevItem.DocId != overrideItem.DocId)
                        {
                            newSettings.Add(settings);
                        }
                    }
                }
            }

            result.DataOverrides = overrideList;
            result.NewSettings = MapToApi(ch, newSettings);

            return result;

        }
    }
}
