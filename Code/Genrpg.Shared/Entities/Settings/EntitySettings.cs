using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameDatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Entities.Settings
{
    [MessagePackObject]
    public class EntitySettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<EntityType> EntityTypes { get; set; }

        public EntityType GetEntityType(long idkey) { return _lookup.Get<EntityType>(idkey); }
    }
}
