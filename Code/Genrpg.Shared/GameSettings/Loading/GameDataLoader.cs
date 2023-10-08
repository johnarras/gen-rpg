using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Loading
{
    public class GameDataLoader<TServer> : IGameSettingsLoader
        where TServer : BaseGameSettings, new()
    {
        public virtual Type GetServerType() { return typeof(TServer); }
        public virtual bool SendToClient() { return true; }

        public virtual async Task<List<IGameSettings>> LoadAll(IRepositorySystem repoSystem, bool createDefaultIfMissing)
        {
            List<IGameSettings> list = (await repoSystem.Search<TServer>(x => true)).Cast<IGameSettings>().ToList();
            if (createDefaultIfMissing && list.Count < 1)
            {
                list.Add(new TServer() { Id = GameDataConstants.DefaultFilename });
            }
            return list;
        }
    }
}
