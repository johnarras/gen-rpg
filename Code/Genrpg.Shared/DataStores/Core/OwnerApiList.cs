using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Units.Data;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Core
{
    [MessagePackObject]
    public class OwnerApiList<Parent,Child> : StubUnitData
        where Parent : OwnerObjectList<Child>, new()
        where Child : OwnerPlayerData
    {
        [Key(0)] public List<Child> Data { get; set; } = new List<Child>();

        public override void AddTo(Unit unit)
        {
            Parent parent = Activator.CreateInstance<Parent>();
            parent.SetData(Data);
            parent.AddTo(unit);
        }

        public override void Delete(IRepositorySystem repoSystem)
        {
            throw new NotImplementedException();
        }

        public override void SaveAll(IRepositorySystem repoSystem)
        {
            throw new NotImplementedException();
        }
    }
}
