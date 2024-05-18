using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Utils;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Dungeons.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.GameSettings.Settings;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.Shared.Login.Messages.RefreshGameSettings;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.PlayerFiltering.Utils;
using Genrpg.Shared.Settings.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Versions.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.GameSettings.Services
{
    public class GameDataService<D> : IGameDataService where D : GameData, new()
    {

        private Dictionary<Type, IGameSettingsLoader> _loaderObjects = null;
        private Dictionary<Type, IGameSettingsMapper> _mapperObjects = null;
        protected IRepositoryService _repoService = null;
        private IGameData _gameData;

        private async Task SetupLoaders(GameState gs, IRepositoryService repoSystem, CancellationToken token)
        {
            if (_loaderObjects != null)
            {
                return;
            }
            List<Type> loadTypes = ReflectionUtils.GetTypesImplementing(typeof(IGameSettingsLoader));

            Dictionary<Type, IGameSettingsLoader> newList = new Dictionary<Type, IGameSettingsLoader>();
            foreach (Type lt in loadTypes)
            {
                if (await ReflectionUtils.CreateInstanceFromType(gs, lt, token) is IGameSettingsLoader newLoader)
                {
                    newList[newLoader.GetServerType()] = newLoader;
                }
            }

            List<Task> setupTasks = new List<Task>();

            foreach (IGameSettingsLoader loader in newList.Values)
            {
                setupTasks.Add(loader.Setup(repoSystem));
            }

            await Task.WhenAll(setupTasks);

            _loaderObjects = newList;
            await Task.CompletedTask;
        }

        private async Task SetupMappers(GameState gs, IRepositoryService repoSystem, CancellationToken token)
        {
            if (_mapperObjects != null)
            {
                return;
            }
            List<Type> mapperTypes = ReflectionUtils.GetTypesImplementing(typeof(IGameSettingsMapper));

            Dictionary<Type, IGameSettingsMapper> newDict = new Dictionary<Type, IGameSettingsMapper>();

            foreach (Type lt in mapperTypes)
            {
                if (await ReflectionUtils.CreateInstanceFromType(gs, lt, token) is IGameSettingsMapper newMapper)
                {
                    newDict[newMapper.GetServerType()] = newMapper;
                }
            }

            _mapperObjects = newDict;
            await Task.CompletedTask;
        }

        public List<IGameSettingsLoader> GetAllLoaders()
        {
            return _loaderObjects.Values.OrderBy(x=>x.GetType().Name).ToList();
        }

        public async Task Initialize(GameState gs, CancellationToken token)
        {
            await SetupLoaders(gs, _repoService, token);
            await SetupMappers(gs, _repoService, token);
        }

        public virtual List<string> GetEditorIgnoreFields()
        {
            return new List<string>() { "_lookup", "DocVersion" };
        }

        public virtual async Task<IGameData> LoadGameData(ServerGameState gs, bool createMissingGameData)
        {
            GameData gameData = new GameData();

            List<Task<List<ITopLevelSettings>>> allTasks = new List<Task<List<ITopLevelSettings>>>();

            foreach (IGameSettingsLoader loader in _loaderObjects.Values)
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

        public bool SetGameDataOverrides(ServerGameState gs, IFilteredObject obj, bool forceRefresh)
        {
            if (!(obj is Character ch))
            {
                return false;
            }

            GameDataOverrideData gameDataOverrideData = ch.Get<GameDataOverrideData>();

            if (gameDataOverrideData.OverrideList == null)
            {
                gameDataOverrideData.OverrideList = new GameDataOverrideList();
            }

            GameDataOverrideList gameDataOverrideList = gameDataOverrideData.OverrideList;

            VersionSettings versionSettings = _gameData.Get<VersionSettings>(ch);

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
                versionSettings.GameDataSaveTime == gameDataOverrideData.GameDataSaveTime &&
                gameDataOverrideData.LastTimeSet >= dataOverrideSettings.PrevUpdateTime &&
                gameDataOverrideData.LastTimeSet < dataOverrideSettings.NextUpdateTime)
            { 
                ch.SetGameDataOverrideList(gameDataOverrideList);
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

            gameDataOverrideData.GameDataSaveTime = versionSettings.GameDataSaveTime;
            gameDataOverrideData.LastTimeSet = DateTime.UtcNow;
            _repoService.QueueSave(gameDataOverrideData);

            gameDataOverrideList.Items = new List<PlayerSettingsOverrideItem>();

            foreach (DataOverrideItemPriority priority in priorityOverrides)
            {
                gameDataOverrideList.Items.Add(new PlayerSettingsOverrideItem()
                {
                    SettingId = settingsNameSettings.Get(priority.SettingsNameId).Name,
                    DocId = priority.DocId,
                });
            }

            gameDataOverrideList.Items = gameDataOverrideList.Items.OrderBy(x => x.SettingId).ToList();

            // This should be deterministic across machines because the player has a set
            // of overrides that should be the same for anyone who's in the same bucket
            // and then the game data save time and the prev update time (last time the
            // overrides changed) will be the same.
            string fullString = SerializationUtils.Serialize(gameDataOverrideList.Items) +
                versionSettings.GameDataSaveTime.Ticks.ToString() + "." +
                dataOverrideSettings.PrevUpdateTime.Ticks.ToString();

            gameDataOverrideList.Hash = PasswordUtils.Sha256(fullString);

            ch.SetGameDataOverrideList(gameDataOverrideList);

            return true;
        }

        public List<ITopLevelSettings> MapToApi(ServerGameState gs, List<ITopLevelSettings> startSettings)
        {
            List<ITopLevelSettings> retval = new List<ITopLevelSettings>();

            foreach (ITopLevelSettings settings in startSettings)
            {
                if (_mapperObjects.TryGetValue(settings.GetType(), out IGameSettingsMapper mapper))
                {
                    retval.Add(mapper.MapToApi(settings));
                }
                else
                {
                    retval.Add(settings);
                }
            }
            return retval;
        }

        public List<ITopLevelSettings> GetClientGameData(ServerGameState gs, IFilteredObject obj, bool sendAllDefault, List<ClientCachedGameSettings> clientCache = null)
        {

            List<ITopLevelSettings> retval = new List<ITopLevelSettings>();
            SetGameDataOverrides(gs, obj, true);

            List<ITopLevelSettings> allData = _gameData.AllSettings();

            foreach (Type t in _mapperObjects.Keys)
            {
                IGameSettingsMapper mapper = _mapperObjects[t];

                if (!mapper.SendToClient())
                {
                    continue;
                }

                string docName = GameDataConstants.DefaultFilename;

                if (!sendAllDefault)
                {
                    docName = _gameData.DataObjectName(t.Name, obj);
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
                    ClientCachedGameSettings clientCachedItem = clientCache.FirstOrDefault(x => x.TypeName == mapper.GetServerType().Name);

                    if (clientCachedItem != null && clientCachedItem.ClientSaveTime >= updateData.UpdateTime)
                    {
                        continue;
                    }

                }
                retval.Add(mapper.MapToApi(currData));
            }
            return retval;
        }

        private bool AcceptedByFilter(Character ch, IPlayerFilter filter)
        {
            if (!PlayerFilterUtils.IsActive(filter))
            {
                return false;
            }

            if (filter.AllowedPlayers.Any(x=>x.PlayerId == ch.Id))
            {
                return true;
            }

            if (filter.MinLevel > 0 && filter.MaxLevel > 0 &&
                (filter.MinLevel > ch.Level || filter.MaxLevel < ch.Level))
            {
                return false;
            }

            if (filter.TotalModSize > 0 && filter.MaxAcceptableModValue > 0)
            {
                long idHash = StrUtils.GetIdHash(filter.IdKey + ch.Id);

                if (idHash % filter.TotalModSize >= filter.MaxAcceptableModValue)
                {
                    return false;
                }
            }


            return true;
        }

        public async Task ReloadGameData(ServerGameState gs)
        {
            IGameData newData = await LoadGameData(gs, true);

            newData.PrevSaveTime = _gameData.CurrSaveTime;
        }

        public RefreshGameSettingsResult GetNewGameDataUpdates(ServerGameState gs, Character ch, bool forceRefresh)
        {
            if (!SetGameDataOverrides(gs, ch, forceRefresh))
            {
                return null;
            }

            RefreshGameSettingsResult result = new RefreshGameSettingsResult();

            List<ITopLevelSettings> newSettings = new List<ITopLevelSettings>();

            // Always load this on client commands/always have in mapserver.
            GameDataOverrideData overrideData = ch.Get<GameDataOverrideData>();

            List<PlayerSettingsOverrideItem> oldPlayerOverrides = new List<PlayerSettingsOverrideItem>(overrideData.OverrideList.Items);

            GameDataOverrideList overrideList = ch.GetOverrideList();

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

            result.Overrides = overrideList;
            result.NewSettings = MapToApi(gs, newSettings);

            return result;

        }
    }
}
