using Genrpg.Shared.Charms.Settings;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Loaders
{
    public class ParentSettingsLoader<TParent, TChild, TApi> : GameSettingsLoader<TParent>
        where TParent : ParentSettings<TChild>, new()
        where TChild : ChildSettings, new()
        where TApi : ParentSettingsApi<TParent, TChild>, new()
    {

        public override Type GetClientType() { return typeof(TApi); }

        public override async Task Setup(IRepositorySystem repoSystem)
        {
            await base.Setup(repoSystem);
            List<IndexConfig> configs = new List<IndexConfig>();
            configs.Add(new IndexConfig() { Ascending = true, MemberName = "ParentId" });
            await repoSystem.CreateIndex<TChild>(configs);
        }

        public override async Task<List<ITopLevelSettings>> LoadAll(IRepositorySystem repoSystem, bool createDefaultIfMissing)
        {
            Task<List<ITopLevelSettings>> loadParents = base.LoadAll(repoSystem, createDefaultIfMissing);

            Task<List<TChild>> loadChildren = repoSystem.Search<TChild>(x => true);

            await Task.WhenAll(loadParents, loadChildren).ConfigureAwait(false);

            List<ITopLevelSettings> settings = await loadParents;
            List<TChild> allChildren = await loadChildren;

            foreach (IGameSettings setting in settings)
            {
                if (setting is TParent parent)
                {
                    parent.SetData(allChildren.Where(x => x.ParentId == parent.Id).ToList());
                }
            }

            return settings;
        }

        public override ITopLevelSettings MapToApi(ITopLevelSettings settings)
        {
            if (settings is TParent tparent)
            {

                TApi api = new TApi()
                {
                    ParentObj = tparent,
                    Data = tparent.GetData().ToList(),
                    Id = tparent.Id,
                };
                return api;
            }
            return settings;
        }
    }
}
