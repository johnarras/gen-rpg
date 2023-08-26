using Genrpg.ServerShared.Core;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData.Loaders
{
    /// <summary>
    /// Class to load and save a list of items that can be individually loaded and saved
    /// </summary>
    /// <typeparam name="Parent">Parent container object (Think InventoryData)</typeparam>
    /// <typeparam name="Child">Child type object (think Items)</typeparam>
    /// <typeparam name="API">Type used to send the parent data to the client (since the Parent has no public list.</typeparam>
    public class OwnerDataLoader<Parent,Child,API> : UnitDataLoader<Parent> 
        where Parent : OwnerObjectList<Child>, new()
        where Child : OwnerPlayerData
        where API : OwnerApiList<Parent,Child>
    {

        public override async Task Setup(ServerGameState gs)
        {
            await base.Setup(gs);

            List<IndexConfig> configs = new List<IndexConfig>();
            configs.Add(new IndexConfig() { Ascending = true, MemberName = "OwnerId" });
            await gs.repo.CreateIndex<Child>(configs);
        }

        public override async Task<IUnitData> LoadData(IRepositorySystem repoSystem, Unit unit)
        {
            Parent parent = await repoSystem.Load<Parent>(unit.Id);
            if (parent != null)
            {
                List<Child> items = await repoSystem.Search<Child>(x => x.OwnerId == unit.Id);
                parent.SetData(items);
            }
            return parent;
        }

        public override IUnitData MapToAPI(IUnitData serverObject)
        {
            Parent parent = serverObject as Parent;

            API api = Activator.CreateInstance<API>();

            api.Data = parent.GetData();

            return api;
        }

        public override void Delete(IRepositorySystem repoSystem, IUnitData data)
        {
            base.Delete(repoSystem, data);
            if (data is Parent parent)
            {
                foreach (Child child in parent.GetData())
                {
                    repoSystem.Delete(child);
                }
            }
        }
    }
}
