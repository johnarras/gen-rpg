using MessagePack;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using Genrpg.Shared.DataStores.Categories.PlayerData;

namespace Genrpg.Shared.DataStores.PlayerData
{
    [MessagePackObject]
    public class OwnerApiList<Parent, Child> : StubUnitData
        where Parent : OwnerObjectList<Child>, new()
        where Child : OwnerPlayerData
    {
        [Key(0)] public List<Child> Data { get; set; } = new List<Child>();
        [Key(1)] public Parent ParentObj { get; set; }

        public override void AddTo(Unit unit)
        {
            unit.Set(ParentObj);
            ParentObj.SetData(Data);
        }
    }
}
