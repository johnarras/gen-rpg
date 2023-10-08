using Genrpg.Shared.DataStores.Categories.WorldData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Maps.Entities
{
    public class MapRoot : BaseWorldData, IMapRoot
    {
        public override string Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }

        public int BlockCount { get; set; }
        public float ZoneSize { get; set; }

        public long Seed { get; set; }

        public int MapVersion { get; set; }

        public int SpawnX { get; set; }
        public int SpawnY { get; set; }

        public float EdgeMountainChance { get; set; }

        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }

    }
}
