using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GameSettings.Entities
{
    [MessagePackObject]
    public class SessionOverrideItem
    {
        [Key(0)] public long Priority { get; set; }
        [Key(1)] public string SettingId { get; set; }
        [Key(2)] public string DocId { get; set; }
    }
}
