using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData
{
    public class PlayerDataService : IPlayerDataService
    {
        protected IServiceLocator _loc;
        protected IRepositoryService _repoService = null;

        SetupDictionaryContainer<Type,IUnitDataLoader> _loaderObjects = new SetupDictionaryContainer<Type, IUnitDataLoader>();
        SetupDictionaryContainer<Type, IUnitDataMapper> _mapperObjects = new SetupDictionaryContainer<Type, IUnitDataMapper>();

        public async Task Initialize(CancellationToken token)
        {
            List<Task> loaderTasks = new List<Task>();
            CreateIndexData data = new CreateIndexData();
            data.Configs.Add(new IndexConfig() { Ascending = true, MemberName = nameof(CoreCharacter.UserId) });
            await _repoService.CreateIndex<CoreCharacter>(data);
            await Task.CompletedTask;
        }

        public Dictionary<Type,IUnitDataLoader> GetLoaders()
        {
            return _loaderObjects.GetDict();
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
                if (_mapperObjects.TryGetValue(serverData.GetType(), out IUnitDataMapper mapper))
                {
                    if (mapper.SendToClient())
                    {
                        retval.Add(mapper.MapToAPI(serverData));
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

        public async Task<List<IUnitData>> LoadAllPlayerData(IRandom rand, User user, Character ch = null)
        {
            bool haveCharacter = ch != null;

            if (!haveCharacter)
            {
                ch = new Character(_repoService) { Id = user.Id, UserId = user.Id };
            }

            List<Task<IUnitData>> allTasks = new List<Task<IUnitData>>();
            foreach (IUnitDataLoader loader in _loaderObjects.GetDict().Values)
            {
                if (haveCharacter || loader.IsUserData())
                {
                    allTasks.Add(LoadOrCreateData(loader, _repoService, ch));
                }
            }

            IUnitData[] dataArray = await Task.WhenAll(allTasks);

            List<IUnitData> dataList = dataArray.ToList();
           
            return dataList;
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
