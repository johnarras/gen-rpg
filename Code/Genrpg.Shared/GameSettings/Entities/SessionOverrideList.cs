using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GameSettings.Entities
{
    [MessagePackObject]
    public class SessionOverrideList
    {
        [Key(0)] public List<SessionOverrideItem> Items { get; set; } = new List<SessionOverrideItem>();      
    }
}
