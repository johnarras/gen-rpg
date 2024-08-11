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
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Loaders
{
    public class ParentSettingsLoader<TParent, TChild> : IGameSettingsLoader
        where TParent : ParentSettings<TChild>, new()
        where TChild : ChildSettings, new()
    {

        protected IRepositoryService _repoService;

        public virtual Version MinClientVersion => new Version();
        public virtual Type GetKey() { return typeof(TParent); }
        public virtual Type GetChildType() { return typeof(TChild); }

        public virtual async Task Initialize(CancellationToken token)
        {
            CreateIndexData data = new CreateIndexData();
            data.Configs.Add(new IndexConfig() { Ascending = true, MemberName = nameof(ChildSettings.ParentId) });
            await _repoService?.CreateIndex<TChild>(data);
        }

        public virtual async Task<List<ITopLevelSettings>> LoadAll(IRepositoryService repoSystem, bool createDefaultIfMissing)
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
    }
}
