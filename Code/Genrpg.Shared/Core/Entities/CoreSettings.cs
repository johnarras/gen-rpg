using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameDatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Core.Entities
{
    [MessagePackObject]
    public class CoreSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string Env { get; set; }
        [Key(2)] public string GameName { get; set; }
        [Key(3)] public string ArtURL { get; set; }
        [Key(4)] public string Version { get; set; }
        [Key(5)] public string UnityProjectId { get; set; }
        [Key(6)] public string BundleId { get; set; }
    }
}
