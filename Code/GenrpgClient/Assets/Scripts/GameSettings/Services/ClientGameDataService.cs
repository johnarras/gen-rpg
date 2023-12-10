using Assets.Scripts.Model;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Reflection.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace Assets.Scripts.GameSettings.Services
{
    public class ClientGameDataService : IClientGameDataService
    {
        private IReflectionService _reflectionService = null;

        private Dictionary<Type, IGameSettingsLoader> _loaderObjects = null;

        public async Task Setup(GameState gs, CancellationToken token)
        {
            List<Type> loaderTypes = _reflectionService.GetTypesImplementing(typeof(IGameSettingsLoader));

            Dictionary<Type, IGameSettingsLoader> newList = new Dictionary<Type, IGameSettingsLoader>();
            foreach (Type lt in loaderTypes)
            {
                if (Activator.CreateInstance(lt) is IGameSettingsLoader newLoader && newLoader.SendToClient())
                {
                    newList[newLoader.GetClientType()] = newLoader;
                }
            }
            _loaderObjects = newList;
            await Task.CompletedTask;
        }
        public async UniTask LoadCachedSettings(UnityGameState gs)
        {
            ClientRepositorySystem repo = gs.repo as ClientRepositorySystem;

            List<IGameSettings> allSettings = new List<IGameSettings>();
            foreach (IGameSettingsLoader loader in _loaderObjects.Values)
            {
                object obj = await repo.LoadWithType(loader.GetClientType(), GameDataConstants.DefaultFilename);
                if (obj is IGameSettings settings)
                {
                    allSettings.Add(settings);
                }
            }
            gs.data.AddData(allSettings);

            await Task.CompletedTask;
        }

        public async UniTask SaveSettings(UnityGameState gs, IGameSettings settings)
        {
            await gs.repo.Save(settings);
        }
    }
}
