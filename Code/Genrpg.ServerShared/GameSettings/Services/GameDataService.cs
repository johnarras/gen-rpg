using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Utils;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.Shared.Login.Messages.RefreshGameData;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Versions.Entities;
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

        private async Task SetupLoaders(IRepositorySystem repoSystem)
        {
            if (_loaderObjects != null)
            {
                return;
            }
            List<Type> loadTypes = ReflectionUtils.GetTypesImplementing(typeof(IGameSettingsLoader));

            Dictionary<Type, IGameSettingsLoader> newList = new Dictionary<Type, IGameSettingsLoader>();
            foreach (Type lt in loadTypes)
            {
                if (Activator.CreateInstance(lt) is IGameSettingsLoader newLoader)
                {
                    newList[newLoader.GetServerType()] = newLoader;
                }
            }
            _loaderObjects = newList;
            await Task.CompletedTask;
        }

        public List<IGameSettingsLoader> GetAllLoaders()
        {
            return _loaderObjects.Values.OrderBy(x=>x.GetType().Name).ToList();
        }

        public async Task Setup(GameState gs, CancellationToken token)
        {
            await SetupLoaders(gs.repo);
        }

        public virtual List<string> GetEditorIgnoreFields()
        {
            return new List<string>() { "_lookup", "DocVersion" };
        }

        public virtual async Task<GameData> LoadGameData(ServerGameState gs, bool createMissingGameData)
        {
            GameData gameData = new GameData();

            List<Task<List<IGameSettings>>> allTasks = new List<Task<List<IGameSettings>>>();

            foreach (IGameSettingsLoader loader in _loaderObjects.Values)
            {
                allTasks.Add(loader.LoadAll(gs.repo, true));
            }

            List<IGameSettings>[] allSettings = await Task.WhenAll(allTasks.ToArray());

            foreach (List<IGameSettings> settingsList in allSettings)
            {
                foreach (IGameSettings settings in settingsList)
                {
                    settings.AddTo(gameData);
                }
            }
            gameData.SetupDataDict(true);

            gameData.CurrSaveTime = gameData.GetGameData<VersionSettings>(null).GameDataSaveTime;

            return gameData;
        }

        public virtual async Task<bool> SaveGameData(GameData data, IRepositorySystem repoSystem)
        {
            DateTime startSaveTime = DateTime.UtcNow;
            D tdata = data as D;
            if (tdata == null)
            {
                return false;
            }

            foreach (IGameSettings baseGameData in data.GetAllData())
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

            VersionSettings versionSettings = gs.data.GetGameData<VersionSettings>(ch);

            DateTime startOfHour = DateTime.Today.AddHours(DateTime.UtcNow.Hour);

            if (!forceRefresh && 
                versionSettings.GameDataSaveTime == gameDataOverrideData.GameDataSaveTime &&
                gameDataOverrideData.LastTimeSet == startOfHour)
            {
                ch.SetGameDataOverrideList(gameDataOverrideList);
                return false;
            }

            List<DataOverrideItemPriority> priorityOverrides = new List<DataOverrideItemPriority>();

            DataOverrideSettings dataOverrideSettings = gs.data.GetGameData<DataOverrideSettings>(null);

            List<DataOverrideGroup> acceptableGroups = new List<DataOverrideGroup>();

            // dataOverrideSettings.GetData() is ordered the DataOverrideSettingsLoader
            foreach (DataOverrideGroup overrideGroup in dataOverrideSettings.GetData())
            {
                if (AcceptedByFilter(ch, overrideGroup))
                {

                    // Each group.Items is ordered on load by SettingsId then by DocId
                    foreach (DataOverrideItem groupItem in overrideGroup.Items)
                    {
                        if (string.IsNullOrEmpty(groupItem.SettingId) ||
                            string.IsNullOrEmpty(groupItem.DocId) ||
                            groupItem.DocId == GameDataConstants.DefaultFilename)
                        {
                            continue;
                        }

                        DataOverrideItemPriority overrideItem = priorityOverrides.FirstOrDefault(x => x.SettingId == groupItem.SettingId);

                        if (overrideItem == null)
                        {
                            overrideItem = new DataOverrideItemPriority { SettingId = groupItem.SettingId };
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
            gameDataOverrideData.LastTimeSet = startOfHour;
            gameDataOverrideData.SetDirty(true);

            gameDataOverrideList.Items = new List<PlayerSettingsOverrideItem>();

            foreach (DataOverrideItemPriority priority in priorityOverrides)
            {
                gameDataOverrideList.Items.Add(new PlayerSettingsOverrideItem()
                {
                    SettingId = priority.SettingId,
                    DocId = priority.DocId,
                });
            }

            gameDataOverrideList.Items = gameDataOverrideList.Items.OrderBy(x => x.SettingId).ToList();

            string fullString = SerializationUtils.Serialize(gameDataOverrideList.Items) +
                gameDataOverrideData.GameDataSaveTime.Ticks.ToString() + "." +
                gameDataOverrideData.LastTimeSet.Ticks.ToString();

            gameDataOverrideList.Hash = PasswordUtils.Sha256(fullString);

            ch.SetGameDataOverrideList(gameDataOverrideList);

            return true;
        }

        public List<IGameSettings> MapToApi(ServerGameState gs, List<IGameSettings> startSettings)
        {
            List<IGameSettings> retval = new List<IGameSettings>();

            foreach (IGameSettings settings in startSettings)
            {
                if (_loaderObjects.TryGetValue(settings.GetType(), out IGameSettingsLoader loader))
                {
                    retval.Add(loader.MapToApi(settings));
                }
                else
                {
                    retval.Add(settings);
                }
            }
            return retval;
        }

        public List<IGameSettings> GetClientGameData(ServerGameState gs, IFilteredObject obj, bool sendAllDefault, List<ClientCachedGameSettings> clientCache = null)
        {

            List<IGameSettings> retval = new List<IGameSettings>();
            SetGameDataOverrides(gs, obj, true);

            List<IGameSettings> allData = gs.data.GetAllData();

            foreach (Type t in _loaderObjects.Keys)
            {
                IGameSettingsLoader loader = _loaderObjects[t];

                if (!loader.SendToClient())
                {
                    continue;
                }

                string docName = GameDataConstants.DefaultFilename;

                if (!sendAllDefault)
                {
                    docName = gs.data.GetDataObjectName(t.Name, obj);
                    if (docName == GameDataConstants.DefaultFilename)
                    {
                        continue;
                    }
                }

                IGameSettings currData = allData.FirstOrDefault(x =>
                x.GetType().Name == t.Name &&
                x.Id == docName);



                if (clientCache != null && docName == GameDataConstants.DefaultFilename &&
                    currData != null && currData is IUpdateData updateData)
                {
                    ClientCachedGameSettings clientCachedItem = clientCache.FirstOrDefault(x => x.TypeName == loader.GetServerType().Name);

                    if (clientCachedItem != null && clientCachedItem.ClientSaveTime >= updateData.UpdateTime)
                    {
                        continue;
                    }

                }
                retval.Add(loader.MapToApi(currData));
            }
            return retval;
        }

        private bool AcceptedByFilter(Character ch, IPlayerFilter filter)
        {
            if (filter.UseDateRange &&
                (filter.StartDate > DateTime.UtcNow ||
                filter.EndDate < DateTime.UtcNow))
            {
                return false;
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
            if (gs.data == null)
            {
                gs.data = await LoadGameData(gs, true);
            }
            else
            {
                GameData newData = await LoadGameData(gs, true);

                newData.PrevSaveTime = gs.data.CurrSaveTime;
                gs.data = newData;
            }
        }

        public RefreshGameSettingsResult GetNewGameDataUpdates(ServerGameState gs, Character ch, bool forceRefresh)
        {
            if (!SetGameDataOverrides(gs, ch, forceRefresh))
            {
                return null;
            }

            RefreshGameSettingsResult result = new RefreshGameSettingsResult();

            List<IGameSettings> newSettings = new List<IGameSettings>();

            // Always load this on client commands/always have in mapserver.
            GameDataOverrideData overrideData = ch.Get<GameDataOverrideData>();

            List<PlayerSettingsOverrideItem> oldPlayerOverrides = new List<PlayerSettingsOverrideItem>(overrideData.OverrideList.Items);

            GameDataOverrideList overrideList = ch.GetGameDataOverrideList();

            List<IGameSettings> allSettings = gs.data.GetAllData();

            foreach (IGameSettings settings in allSettings)
            {
                BaseGameSettings baseSettings = settings as BaseGameSettings;

                if (baseSettings.Id == GameDataConstants.DefaultFilename)
                {
                    // If it's default data saved after the current save time, then send it to the client.
                    if (baseSettings.UpdateTime >= gs.data.CurrSaveTime)
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

                        if (prevItem == null || baseSettings.UpdateTime >= gs.data.CurrSaveTime ||
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
