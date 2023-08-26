using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GameDatas.Config
{
    [MessagePackObject]
    public class ConfigItem
    {
        [Key(0)] public string SettingId { get; set; }
        [Key(1)] public string DocId { get; set; }
    }
}
