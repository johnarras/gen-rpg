using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.PlayerData.LoadUpdateHelpers;
using Genrpg.Shared.AI.Settings;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.SpellCrafting.Services;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Spells.Settings.Spells;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Utils;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData
{
    public class PlayerDataService : IPlayerDataService
    {
        private Dictionary<Type, IUnitDataLoader> _loaderObjects = null;

        private List<ICharacterLoadUpdater> _loadUpdateHelpers = new List<ICharacterLoadUpdater>();

        public async Task Setup(GameState gs, CancellationToken token)
        {
            List<IndexConfig> configs = new List<IndexConfig>();
            configs.Add(new IndexConfig() { Ascending = true, MemberName = "UserId" });
            await gs.repo.CreateIndex<CoreCharacter>(configs);

            List<Type> loadTypes = ReflectionUtils.GetTypesImplementing(typeof(IUnitDataLoader));

            Dictionary<Type, IUnitDataLoader> newList = new Dictionary<Type, IUnitDataLoader>();
            foreach (Type lt in loadTypes)
            {
                if (Activator.CreateInstance(lt) is IUnitDataLoader newLoader)
                {
                    newList[newLoader.GetServerType()] = newLoader;
                    await newLoader.Setup(gs);
                }
            }

            _loaderObjects = newList;

            List<Type> updateTypes = ReflectionUtils.GetTypesImplementing(typeof(ICharacterLoadUpdater));
            _loadUpdateHelpers = new List<ICharacterLoadUpdater>();

            foreach (Type ut in updateTypes)
            {
                if (Activator.CreateInstance(ut) is ICharacterLoadUpdater helper)
                {
                    _loadUpdateHelpers.Add(helper);
                    await helper.Setup(gs);
                }
            }

            _loadUpdateHelpers = _loadUpdateHelpers.OrderBy(x => x.Priority).ToList();
        }

        public Dictionary<Type,IUnitDataLoader> GetLoaders()
        {
            return _loaderObjects;
        }

        public IUnitDataLoader GetLoader<T>() where T : IUnitData
        {
            if (_loaderObjects.TryGetValue(typeof(T), out IUnitDataLoader loader))
            {
                return loader;
            }
            return null;
        }

        public void SavePlayerData(Character ch, IRepositorySystem repoSystem, bool saveClean)
        {
            ch?.SaveAll(repoSystem, saveClean);
        }

        public async Task<List<IUnitData>> MapToClientApi(List<IUnitData> serverDataList)
        {
            List<IUnitData> retval = new List<IUnitData>();

            foreach (IUnitData serverData in serverDataList)
            {
                if (_loaderObjects.TryGetValue(serverData.GetType(), out IUnitDataLoader loader))
                {
                    if (loader.SendToClient())
                    {
                        retval.Add(loader.MapToAPI(serverData));
                    }
                }
            }
            await Task.CompletedTask;
            return retval;
        }

        public async Task<List<IUnitData>> LoadPlayerData(ServerGameState gs, Character ch)
        {
            List<Task<IUnitData>> allTasks = new List<Task<IUnitData>>();
            foreach (IUnitDataLoader loader in _loaderObjects.Values)
            {
                allTasks.Add(LoadOrCreateData(loader, gs.repo, ch));
            }

            IUnitData[] allData = await Task.WhenAll(allTasks.ToList());

            foreach (IUnitData data in allData)
            {
                data.AddTo(ch);
            }

            UpdateOnLoad(gs, ch);
            return allData.ToList();
        }

        protected async Task<IUnitData> LoadOrCreateData(IUnitDataLoader loader, IRepositorySystem repoSystem, Character ch)
        {
            IUnitData newData = await loader.LoadData(repoSystem, ch);
            if (newData == null)
            {
                newData = loader.Create(ch);
            }
            return newData;
        }

        protected async void UpdateOnLoad(ServerGameState gs, Character ch)
        {
            foreach (ICharacterLoadUpdater updater in _loadUpdateHelpers)
            {
                await updater.Update(gs, ch);
            }
          
        }

        public async Task<List<CharacterStub>> LoadCharacterStubs(ServerGameState gs, string userId)
        {
            // TODO: projection in the repo itself
            List<CoreCharacter> chars = await gs.repo.Search<CoreCharacter>(x => x.UserId == userId);

            List<CharacterStub> stubs = new List<CharacterStub>();
            foreach (CoreCharacter ch in chars)
            {
                stubs.Add(new CharacterStub()
                {
                    Id = ch.Id,
                    Name = ch.Name,
                    Level = ch.Level,
                });
            }

            return stubs;
        }

    }
}
