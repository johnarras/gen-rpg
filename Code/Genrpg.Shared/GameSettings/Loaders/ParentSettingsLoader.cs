using Genrpg.Shared.Charms.Settings;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
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
    public class ParentSettingsLoader<TParent, TChild, TApi> : IGameSettingsLoader
        where TParent : ParentSettings<TChild>, new()
        where TChild : ChildSettings, new()
        where TApi : ParentSettingsApi<TParent, TChild>, new()
    {

        public virtual Type GetServerType() { return typeof(TParent); }
        public virtual Type GetClientType() { return typeof(TApi); }
        public virtual bool SendToClient() { return true; }

        public virtual async Task Setup(IRepositorySystem repoSystem)
        {
            List<IndexConfig> configs = new List<IndexConfig>();
            configs.Add(new IndexConfig() { Ascending = true, MemberName = "ParentId" });
            await repoSystem.CreateIndex<TChild>(configs);
        }

        public virtual async Task<List<ITopLevelSettings>> LoadAll(IRepositorySystem repoSystem, bool createDefaultIfMissing)
        {

            Task<List<TParent>> loadParentsTask = repoSystem.Search<TParent>(x => true);

            Task <List<TChild>> loadChildrenTask = repoSystem.Search<TChild>(x => true);

            await Task.WhenAll(loadParentsTask, loadChildrenTask).ConfigureAwait(false);

            List<TParent> parents = await loadParentsTask;
            List<TChild> allChildren = await loadChildrenTask;

            if (createDefaultIfMissing)
            {
                TParent defaultObject = parents.FirstOrDefault(x => x.Id == GameDataConstants.DefaultFilename);
                if (defaultObject == null)
                {
                    defaultObject = new TParent() { Id = GameDataConstants.DefaultFilename };
                    parents.Add(defaultObject);
                }
            }

            foreach (TParent parent in parents)
            {
                parent.SetData(allChildren.Where(x => x.ParentId == parent.Id).ToList());
            }

            return parents.Cast<ITopLevelSettings>().ToList();
        }

        public virtual ITopLevelSettings MapToApi(ITopLevelSettings settings)
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
