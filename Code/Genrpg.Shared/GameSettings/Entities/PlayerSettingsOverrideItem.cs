using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GameSettings.Entities
{
    [MessagePackObject]
    public class PlayerSettingsOverrideItem
    {
        [Key(0)] public string SettingId { get; set; }
        [Key(1)] public string DocId { get; set; }
    }
}
