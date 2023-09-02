using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameSettings;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Versions.Entities
{
    [MessagePackObject]
    public class VersionSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int ClientVersion { get; set; }
        [Key(2)] public int ServerVersion { get; set; }
        [Key(3)] public int GameDataVersion { get; set; }
        [Key(4)] public int UserVersion { get; set; }
        [Key(5)] public int CharacterVersion { get; set; }
    }
}
