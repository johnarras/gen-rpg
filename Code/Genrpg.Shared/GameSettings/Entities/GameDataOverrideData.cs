using Genrpg.Shared.DataStores.Categories.PlayerData;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GameSettings.Entities
{
    [MessagePackObject]
    public class GameDataOverrideData : BasePlayerData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public DateTime GameDataSaveTime { get; set; } = DateTime.UtcNow;

        [Key(2)] public DateTime LastTimeSet { get; set; } = DateTime.MinValue;

        [Key(3)] public GameDataOverrideList OverrideList { get; set; }

    }
}
