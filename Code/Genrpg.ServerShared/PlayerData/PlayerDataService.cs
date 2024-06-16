using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.PlayerData.LoadUpdateHelpers;
using Genrpg.Shared.AI.Settings;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.SpellCrafting.Services;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Spells.Settings.Spells;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData
{
    public class PlayerDataService : IPlayerDataService
    {
        protected IServiceLocator _loc;
        protected IRepositoryService _repoService = null;
        private Dictionary<Type, IUnitDataLoader> _loaderObjects = null;
        private Dictionary<Type, IUnitDataMapper> _mapperObjects = null;
        private List<ICharacterLoadUpdater> _loadUpdateHelpers = new List<ICharacterLoadUpdater>();

        public async Task Initialize(CancellationToken token)
        {
            List<Task> loaderTasks = new List<Task>();
            List<IndexConfig> configs = new List<IndexConfig>();
            configs.Add(new IndexConfig() { Ascending = true, MemberName = nameof(CoreCharacter.UserId) });
            loaderTasks.Add(_repoService.CreateIndex<CoreCharacter>(configs));

            List<Type> loadTypes = ReflectionUtils.GetTypesImplementing(typeof(IUnitDataLoader));

            Dictionary<Type, IUnitDataLoader> newList = new Dictionary<Type, IUnitDataLoader>();

            foreach (Type lt in loadTypes)
            {
                if (await ReflectionUtils.CreateInstanceFromType(_loc, lt, token) is IUnitDataLoader newLoader)
                {
                    newList[newLoader.GetServerType()] = newLoader;
                    loaderTasks.Add(newLoader.Initialize(token));
                }
            }

            _loaderObjects = newList;

            List<Type> mapperTypes = ReflectionUtils.GetTypesImplementing(typeof(IUnitDataMapper));

            Dictionary<Type, IUnitDataMapper> mapperDict = new Dictionary<Type, IUnitDataMapper>();

            foreach (Type mt in mapperTypes)
            {
               if (await ReflectionUtils.CreateInstanceFromType(_loc,mt,token) is IUnitDataMapper mapper)
                {
                    mapperDict[mapper.GetServerType()] = mapper;
                    loaderTasks.Add(mapper.Initialize(token));
                }
            }

            _mapperObjects = mapperDict;


            List<Type> updateTypes = ReflectionUtils.GetTypesImplementing(typeof(ICharacterLoadUpdater));
            _loadUpdateHelpers = new List<ICharacterLoadUpdater>();

            foreach (Type ut in updateTypes)
            {
                if (await ReflectionUtils.CreateInstanceFromType(_loc, ut, token) is ICharacterLoadUpdater helper)
                {
                    _loadUpdateHelpers.Add(helper);
                }
            }

            await Task.WhenAll(loaderTasks);

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

        public void SavePlayerData(Character ch, bool saveAll)
        {
            ch?.SaveData(saveAll);
        }

        public async Task<List<IUnitData>> MapToClientApi(List<IUnitData> serverDataList)
        {
            List<IUnitData> retval = new List<IUnitData>();

            foreach (IUnitData serverData in serverDataList)
            {
                if (_mapperObjects.TryGetValue(serverData.GetType(), out IUnitDataMapper loader))
                {
                    if (loader.SendToClient())
                    {
                        retval.Add(loader.MapToAPI(serverData));
                    }
                }
                else
                {
                    Console.WriteLine("Missing mapper: " + serverData.GetType().Name);
                }
            }
            await Task.CompletedTask;
            return retval;
        }

        public async Task<T> LoadTopLevelData<T> (Character ch) where T : class, ITopLevelUnitData, new()
        { 
            IUnitDataLoader loader = GetLoader<T>();

            if (loader != null)
            {
                return (T)await loader.LoadTopLevelData(ch);
            }
            return default;
        }

        public async Task<List<IUnitData>> LoadAllPlayerData(IRandom rand, Character ch)
        {
            List<Task<IUnitData>> allTasks = new List<Task<IUnitData>>();
            foreach (IUnitDataLoader loader in _loaderObjects.Values)
            {
                allTasks.Add(LoadOrCreateData(loader, _repoService, ch));
            }

            IUnitData[] allData = await Task.WhenAll(allTasks.ToList());

            foreach (IUnitData data in allData)
            {
                data.AddTo(ch);
            }

            UpdateOnLoad(rand, ch);
            return allData.ToList();
        }

        protected async Task<IUnitData> LoadOrCreateData(IUnitDataLoader loader, IRepositoryService repoSystem, Character ch)
        {
            IUnitData newData = await loader.LoadFullData(ch);
            if (newData == null)
            {
                newData = loader.Create(ch);
            }
            return newData;
        }

        protected async void UpdateOnLoad(IRandom rand, Character ch)
        {
            foreach (ICharacterLoadUpdater updater in _loadUpdateHelpers)
            {
                await updater.Update(rand, ch);
            }
          
        }

        public async Task<List<CharacterStub>> LoadCharacterStubs(string userId)
        {
            // TODO: projection in the repo itself
            List<CoreCharacter> chars = await _repoService.Search<CoreCharacter>(x => x.UserId == userId);

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
