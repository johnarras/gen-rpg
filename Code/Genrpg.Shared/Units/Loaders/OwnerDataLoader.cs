using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Loaders
{
    /// <summary>
    /// Class to load and save a list of items that can be individually loaded and saved
    /// </summary>
    /// <typeparam name="TParent">Parent container object (Think InventoryData)</typeparam>
    /// <typeparam name="TChild">Child type object (think Items)</typeparam>
    /// <typeparam name="TApi">Type used to send the parent data to the client (since the Parent has no public list.</typeparam>
    public class OwnerDataLoader<TParent, TChild> : UnitDataLoader<TParent>
        where TParent : OwnerObjectList<TChild>, new()
        where TChild : OwnerPlayerData, IChildUnitData
    {

        protected IRepositoryService _repoSystem = null;
        public override async Task Initialize(GameState gs, CancellationToken token)
        {
            await base.Initialize(gs,token);

            List<IndexConfig> configs = new List<IndexConfig>();
            configs.Add(new IndexConfig() { Ascending = true, MemberName = "OwnerId" });
            await _repoSystem.CreateIndex<TChild>(configs);
        }

        public override async Task<ITopLevelUnitData> LoadFullData(Unit unit)
        {
            string id = unit.Id;

            if (IsUserData() && unit is Character ch)
            {
                id = ch.UserId;
            }

            Task<TParent> parentTask = _repoSystem.Load<TParent>(id);
            Task<List<TChild>> childTask = _repoSystem.Search<TChild>(x => x.OwnerId == id);

            await Task.WhenAll(parentTask, childTask).ConfigureAwait(false);

            TParent parent = await parentTask;
            List<TChild> items = await childTask;
            if (parent != null) 
            {                
                parent.SetData(items);
            }
            return parent;
        }

        public override async Task<IChildUnitData> LoadChildByIdkey(Unit unit, long idkey)
        {
            await Task.CompletedTask;
            return default;
        }

        public override async Task<IChildUnitData> LoadChildById(Unit unit, string id)
        {
            TParent parentObj = (TParent)await LoadTopLevelData(unit);

            TChild child = parentObj.GetData().FirstOrDefault(x => x.Id == id);

            if (child != null)
            {
                List<TChild> currList = parentObj.GetData().ToList();
                currList.Add(child);
                parentObj.SetData(currList);

            }

            return child;
        }

    }
}
