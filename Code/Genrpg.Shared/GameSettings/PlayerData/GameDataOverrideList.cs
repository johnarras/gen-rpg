using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GameSettings.PlayerData
{
    [MessagePackObject]
    public class PlayerSettingsOverrideItem
    {
        [Key(0)] public string SettingId { get; set; }
        [Key(1)] public string DocId { get; set; }
    }
    [MessagePackObject]
    public class GameDataOverrideList
    {
        [Key(0)] public List<PlayerSettingsOverrideItem> Items { get; set; } = new List<PlayerSettingsOverrideItem>();

        [Key(1)] public string Hash { get; set; }

        [Key(2)] public DateTime GameDataSaveTime { get; set; } = DateTime.UtcNow;
        [Key(3)] public DateTime LastTimeSet { get; set; } = DateTime.MinValue;
    }
}
