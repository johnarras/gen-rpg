using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Loaders
{
    public class ParentSettingsLoader<TParent, TChild, TApi> : GameSettingsLoader<TParent>
        where TParent : ParentSettings<TChild>, new()
        where TChild : ChildSettings, new()
        where TApi : ParentSettingsApi<TParent, TChild>, new()
    {

        public override async Task<List<IGameSettings>> LoadAll(IRepositorySystem repoSystem, bool createDefaultIfMissing)
        {
            Task<List<IGameSettings>> loadParents = base.LoadAll(repoSystem, createDefaultIfMissing);

            Task<List<TChild>> loadChildren = repoSystem.Search<TChild>(x => true);

            await Task.WhenAll(loadParents, loadChildren).ConfigureAwait(false);

            List<IGameSettings> settings = await loadParents;
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

        public override IGameSettings MapToApi(IGameSettings settings)
        {
            if (settings is TParent tparent)
            {

                TApi api = new TApi()
                {
                    ParentObj = tparent,
                    Data = tparent.GetData(),
                };
                return api;
            }
            return settings;
        }
    }
}
