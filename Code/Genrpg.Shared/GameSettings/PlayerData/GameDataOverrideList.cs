using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GameSettings.PlayerData
{
    [MessagePackObject]
    public class GameDataOverrideList
    {
        [Key(0)] public List<PlayerSettingsOverrideItem> Items { get; set; } = new List<PlayerSettingsOverrideItem>();

        [Key(1)] public string Hash { get; set; }
    }
}
