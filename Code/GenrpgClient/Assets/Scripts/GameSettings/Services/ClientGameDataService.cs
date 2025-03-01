using Assets.Scripts.Model;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

using System.Threading.Tasks;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Mappers;
using UnityEngine;
using System.IO;
using Genrpg.Shared.Logging.Interfaces;
using Assets.Scripts.GameSettings.Entities;
using Genrpg.Shared.Client.Core;
using Assets.Scripts.Assets;

namespace Assets.Scripts.GameSettings.Services
{
    public class ClientGameDataService : IClientGameDataService
    {

        private IRepositoryService _repoService;
        protected IGameData _gameData;
        private ILogService _logService;
        private IClientAppService _clientAppService;
        private IClientConfigContainer _configContainer;
        private ILocalLoadService _localLoadService;

        private Dictionary<Type, IGameSettingsMapper> _loaderObjects = null;

        protected string GetFullBakedGameDataPath()
        {
            return _clientAppService.DataPath + "/Resources/" + BakedGameDataPathSuffix;
        }

        public async Task Initialize(CancellationToken token)
        {
            List<Type> mapperTypes = ReflectionUtils.GetTypesImplementing(typeof(IGameSettingsMapper));

            Dictionary<Type, IGameSettingsMapper> newList = new Dictionary<Type, IGameSettingsMapper>();
            foreach (Type lt in mapperTypes)
            {
                if (Activator.CreateInstance(lt) is IGameSettingsMapper newLoader && newLoader.SendToClient())
                {
                    newList[newLoader.GetClientType()] = newLoader;
                }
            }
            _loaderObjects = newList;
            await Task.CompletedTask;
        }
#if UNITY_EDITOR
        
#endif
        const string BakedGameDataPathSuffix = "BakedGameData/";
        public async Awaitable LoadCachedSettings(IClientGameState gs)
        {
            GameData gameData = new ClientGameData();
            ClientRepositoryService repo = _repoService as ClientRepositoryService;

            List<ITopLevelSettings> allSettings = new List<ITopLevelSettings>();
            foreach (IGameSettingsMapper loader in _loaderObjects.Values)
            {
                ITopLevelSettings bakedSettings = null;

                string bakedResourcesPath = BakedGameDataPathSuffix + loader.GetClientType().Name;

               
                TextAsset textAsset = _localLoadService.LocalLoad<TextAsset>(bakedResourcesPath);

                if (textAsset != null && !string.IsNullOrEmpty(textAsset.text))
                {
                    bakedSettings = (ITopLevelSettings)SerializationUtils.DeserializeWithType(textAsset.text, loader.GetClientType());
                }

                if (!_configContainer.Config.PlayerContainsAllAssets)
                {
                    List<ITopLevelSettings> settingsChoices = new List<ITopLevelSettings>();

                    object obj = await repo.LoadWithType(loader.GetClientType(), GameDataConstants.DefaultFilename);

                    ITopLevelSettings downloadedSettings = obj as ITopLevelSettings;

                    // If baked settings are newer than the cached downloaded settings, use the new baked data in place of the cached.
                    // This comes up if you create a new client.
                    if (bakedSettings != null && downloadedSettings != null &&
                        bakedSettings.UpdateTime > downloadedSettings.UpdateTime)
                    {
                        downloadedSettings = bakedSettings;
                    }

                    if (downloadedSettings != null)
                    {
                        allSettings.Add(downloadedSettings);
                    }
                    else if (bakedSettings != null)
                    {
                        allSettings.Add(bakedSettings);
                    }
                }
                else
                {
                    allSettings.Add(bakedSettings);
                }

            }
            gameData.AddData(allSettings);
            _gameData.CopyFrom(gameData);

            await Task.CompletedTask;
        }

        public async Awaitable SaveSettings(IGameSettings settings)
        {
            await _repoService.Save(settings);

#if UNITY_EDITOR

            string dirName = GetFullBakedGameDataPath();

            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            string path = dirName + settings.GetType().Name + ".txt";

            string serializedData = SerializationUtils.PrettyPrint(settings);

            File.WriteAllText(path, serializedData);
#endif
        }
    }
}
