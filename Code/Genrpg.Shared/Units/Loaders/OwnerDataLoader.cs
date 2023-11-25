using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Loaders
{
    /// <summary>
    /// Class to load and save a list of items that can be individually loaded and saved
    /// </summary>
    /// <typeparam name="TParent">Parent container object (Think InventoryData)</typeparam>
    /// <typeparam name="TChild">Child type object (think Items)</typeparam>
    /// <typeparam name="TApi">Type used to send the parent data to the client (since the Parent has no public list.</typeparam>
    public class OwnerDataLoader<TParent, TChild, TApi> : UnitDataLoader<TParent>
        where TParent : OwnerObjectList<TChild>, new()
        where TChild : OwnerPlayerData
        where TApi : OwnerApiList<TParent, TChild>
    {

        public override async Task Setup(GameState gs)
        {
            await base.Setup(gs);

            List<IndexConfig> configs = new List<IndexConfig>();
            configs.Add(new IndexConfig() { Ascending = true, MemberName = "OwnerId" });
            await gs.repo.CreateIndex<TChild>(configs);
        }

        public override async Task<IUnitData> LoadData(IRepositorySystem repoSystem, Unit unit)
        {
            string id = unit.Id;

            if (IsUserData() && unit is Character ch)
            {
                id = ch.UserId;
            }

            Task<TParent> parentTask = repoSystem.Load<TParent>(id);
            Task<List<TChild>> childTask = repoSystem.Search<TChild>(x => x.OwnerId == id);

            await Task.WhenAll(parentTask, childTask).ConfigureAwait(false);

            TParent parent = await parentTask;
            List<TChild> items = await childTask;
            if (parent != null) 
            {                
                parent.SetData(items);
            }
            return parent;
        }

        public override IUnitData MapToAPI(IUnitData serverObject)
        {
            TParent parent = serverObject as TParent;

            TApi api = Activator.CreateInstance<TApi>();

            api.ParentObj = parent;
            api.Data = parent.GetData();

            return api;
        }
    }
}
