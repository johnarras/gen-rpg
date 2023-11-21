using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Loaders
{
    /// <summary>
    /// Class to load and save a list of items that can be individually loaded and saved
    /// </summary>
    /// <typeparam name="Parent">Parent container object (Think InventoryData)</typeparam>
    /// <typeparam name="Child">Child type object (think Items)</typeparam>
    /// <typeparam name="API">Type used to send the parent data to the client (since the Parent has no public list.</typeparam>
    public class OwnerDataLoader<Parent, Child, API> : UnitDataLoader<Parent>
        where Parent : OwnerObjectList<Child>, new()
        where Child : OwnerPlayerData
        where API : OwnerApiList<Parent, Child>
    {

        public override async Task Setup(GameState gs)
        {
            await base.Setup(gs);

            List<IndexConfig> configs = new List<IndexConfig>();
            configs.Add(new IndexConfig() { Ascending = true, MemberName = "OwnerId" });
            await gs.repo.CreateIndex<Child>(configs);
        }

        public override async Task<IUnitData> LoadData(IRepositorySystem repoSystem, Unit unit)
        {
            string id = unit.Id;

            if (IsUserData() && unit is Character ch)
            {
                id = ch.UserId;
            }

            Parent parent = await repoSystem.Load<Parent>(id);
            if (parent != null)
            {
                List<Child> items = await repoSystem.Search<Child>(x => x.OwnerId == id);
                parent.SetData(items);
            }
            return parent;
        }

        public override IUnitData MapToAPI(IUnitData serverObject)
        {
            Parent parent = serverObject as Parent;

            API api = Activator.CreateInstance<API>();

            api.ParentObj = parent;
            api.Data = parent.GetData();

            return api;
        }
    }
}
