﻿using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.GameSettings.Interfaces;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
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

        private List<IGameDataContainer> _gameDataContainers = new List<IGameDataContainer>();

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

        public void AddGameDataContainer(IGameDataContainer container)
        {
            if (container != null)
            {
                _gameDataContainers.Add(container);
            }
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

        public void SetGameDataOverrides(ServerGameState gs, IFilteredObject obj, bool forceRefresh)
        {
            if (!(obj is Character ch))
            {
                return;
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
                return;
            }

            DataOverrideSettings dataOverrideSettings = gs.data.GetGameData<DataOverrideSettings>(null);

            List<DataOverrideGroup> acceptableGroups = new List<DataOverrideGroup>();

            foreach (DataOverrideGroup group in dataOverrideSettings.GetData())
            {
                if (AcceptedByFilter(ch, group))
                {
                    foreach (DataOverrideItem item in group.Items)
                    {
                        if (string.IsNullOrEmpty(item.SettingId) ||
                            string.IsNullOrEmpty(item.DocId) ||
                            item.DocId == GameDataConstants.DefaultFilename)
                        {
                            continue;
                        }

                        PlayerSettingsOverrideItem overrideItem = gameDataOverrideList.Items.FirstOrDefault(x => x.SettingId == item.SettingId);

                        if (overrideItem == null)
                        {
                            overrideItem = new PlayerSettingsOverrideItem() { SettingId = item.SettingId };
                            gameDataOverrideList.Items.Add(overrideItem);
                            overrideItem.DocId = item.DocId;
                        }
                        else if (group.Priority > overrideItem.Priority)
                        {
                            overrideItem.Priority = group.Priority;
                            overrideItem.DocId = item.DocId;
                        }
                    }
                }
            }

            gameDataOverrideData.GameDataSaveTime = versionSettings.GameDataSaveTime;
            gameDataOverrideData.LastTimeSet = startOfHour;
            gameDataOverrideData.SetDirty(true);
            gameDataOverrideList.Items = gameDataOverrideList.Items.OrderBy(x => x.SettingId).ToList();

            ch.SetGameDataOverrideList(gameDataOverrideList);
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

        public List<IGameSettings> GetClientGameData(ServerGameState gs, IFilteredObject obj, bool sendAllDefault)
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

                if (currData != null)
                {
                    retval.Add(loader.MapToApi(currData));
                }
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

            foreach (IGameDataContainer container in _gameDataContainers)
            {
                container.UpdateFromNewGameData(gs.data);
            }
        }
    }
}