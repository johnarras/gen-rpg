using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Loaders
{
    public abstract class NoChildSettingsLoader<TServer> : IGameSettingsLoader where TServer : NoChildSettings, new()
    {
        public virtual Version MinClientVersion => new Version();
        public virtual Type GetChildType() { return typeof(TServer); }
        public virtual bool SendToClient() { return true; }
        public virtual Type GetKey() { return typeof(TServer); }
        public virtual async Task Initialize(CancellationToken token) { await Task.CompletedTask; }

        public virtual async Task<List<ITopLevelSettings>> LoadAll(IRepositoryService repoSystem, bool createDefaultIfMissing)
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
