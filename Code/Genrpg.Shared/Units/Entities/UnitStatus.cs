using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.WorldData;
using Genrpg.Shared.MapObjects.MapObjectAddons.Constants;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;

namespace Genrpg.Shared.Units.Entities
{

    [MessagePackObject]
    public class UnitStatus : BaseWorldData, IId, IStringOwnerId
    {
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string OwnerId { get; set; }
        [Key(2)] public string ObjId { get; set; }
        [Key(3)] public long IdKey { get; set; }
        [Key(4)] public string MapId { get; set; }

        [Key(5)] public List<IMapObjectAddon> Addons { get; set; } = new List<IMapObjectAddon>();

    }
}
