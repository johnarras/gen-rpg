using Assets.Scripts.Model;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Mappers;

namespace Assets.Scripts.GameSettings.Services
{
    public class ClientGameDataService : IClientGameDataService
    {

        private IRepositoryService _repoService;
        protected IGameData _gameData;

        private Dictionary<Type, IGameSettingsMapper> _loaderObjects = null;

        public async Task Initialize(IGameState gs, CancellationToken token)
        {
            List<Type> mapperTypes = ReflectionUtils.GetTypesImplementing(typeof(IGameSettingsMapper));

            Dictionary<Type, IGameSettingsMapper> newList = new Dictionary<Type, IGameSettingsMapper>();
            foreach (Type lt in mapperTypes)
            {
                if (Activator.CreateInstance(lt) is IGameSettingsMapper newLoader && newLoader.SendToClient())
                {
                    newList[newLoader.GetServerType()] = newLoader;
                }
            }
            _loaderObjects = newList;
            await Task.CompletedTask;
        }
        public async UniTask LoadCachedSettings(IUnityGameState gs)
        {
            GameData gameData = new GameData();
            ClientRepositoryService repo = _repoService as ClientRepositoryService;

            List<ITopLevelSettings> allSettings = new List<ITopLevelSettings>();
            foreach (IGameSettingsMapper loader in _loaderObjects.Values)
            {
                object obj = await repo.LoadWithType(loader.GetServerType(), GameDataConstants.DefaultFilename);
                if (obj is ITopLevelSettings settings)
                {
                    allSettings.Add(settings);
                }
            }
            gameData.AddData(allSettings);
            _gameData.CopyFrom(gameData);

            await Task.CompletedTask;
        }

        public async UniTask SaveSettings(IGameSettings settings)
        {
            await _repoService.Save(settings);
        }
    }
}
