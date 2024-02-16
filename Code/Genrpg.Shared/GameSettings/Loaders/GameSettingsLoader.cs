using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Loaders
{
    public class GameSettingsLoader<TServer> : IGameSettingsLoader
        where TServer : BaseGameSettings, ITopLevelSettings, new()
    {
        public virtual Type GetServerType() { return typeof(TServer); }
        public virtual Type GetClientType() { return typeof(TServer); }
        public virtual bool SendToClient() { return true; }

        public virtual async Task Setup(IRepositorySystem repoSystem)
        {
            await Task.CompletedTask;
        }

        public virtual async Task<List<ITopLevelSettings>> LoadAll(IRepositorySystem repoSystem, bool createDefaultIfMissing)
        {
            List<ITopLevelSettings> list = (await repoSystem.Search<TServer>(x => true)).Cast<ITopLevelSettings>().ToList();
            if (createDefaultIfMissing && list.Count < 1)
            {
                list.Add(new TServer() { Id = GameDataConstants.DefaultFilename });              
            }
            return list;
        }

        public virtual ITopLevelSettings MapToApi(ITopLevelSettings settings)
        {
            return settings;
        }
    }
}
