using Genrpg.ServerShared.Core;
using Genrpg.Shared.AI.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Config;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Names.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Microsoft.Extensions.Azure;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.GameSettings.Services
{



    public class GameDataService<D> : IGameDataService where D : GameData, new()
    {
        private Dictionary<Type, IGameDataLoader> _loaderObjects = null;

        private async void SetupLoaders(IRepositorySystem repoSystem)
        {
            if (_loaderObjects != null)
            {
                return;
            }
            List<Type> loadTypes = ReflectionUtils.GetTypesImplementing(typeof(IGameDataLoader));

            Dictionary<Type, IGameDataLoader> newList = new Dictionary<Type, IGameDataLoader>();
            foreach (Type lt in loadTypes)
            {
                if (Activator.CreateInstance(lt) is IGameDataLoader newLoader)
                {
                    newList[newLoader.GetServerType()] = newLoader;
                }
            }
            _loaderObjects = newList;
            await Task.CompletedTask;
        }

        public List<IGameDataLoader> GetAllLoaders()
        {
            return _loaderObjects.Values.ToList();
        }

        public async Task Setup(GameState gs, CancellationToken token)
        {
            SetupLoaders(gs.repo);
            await Task.CompletedTask;
        }

        public GameDataService()
        {
        }

        public virtual List<string> GetEditorIgnoreFields()
        {
            return new List<string>();
        }

        private DataConfig _config = null;
        public async Task<DataConfig> GetDataConfig(ServerGameState gs)
        {
            if (_config == null)
            {
                _config = await gs.repo.Load<DataConfig>(gs.config.DataConfigDocId);
            }

            return _config;
        }

        public virtual async Task<GameData> LoadGameData(ServerGameState gs, bool createMissingGameData)
        {

            DataConfig config = await GetDataConfig(gs);

            GameData gameData = new GameData();

            if (config == null)
            {
                return gameData;
            }

            foreach (IGameDataLoader loader in _loaderObjects.Values)
            {
                ConfigItem item = config.Items.FirstOrDefault(x => x.SettingId == loader.GetTypeName());

                if (item == null)
                {
                    item = new ConfigItem() { DocId = DataConstants.DefaultFilename, SettingId = loader.GetTypeName() };
                    config.Items.Add(item);
                    await gs.repo.Save(config);
                }

                BaseGameData loadedData = await loader.LoadData(gs.repo, item.DocId, createMissingGameData);
                if (loadedData != null)
                {
                    loadedData.Set(gameData);
                }
            }

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

            foreach (IGameSettingsContainer cont in data.GetContainers())
            {
                await cont.SaveData(repoSystem);
            }
            return true;
        }

        public virtual void UpdateDataBeforeSave(GameData data)
        {
        }
    }
}
