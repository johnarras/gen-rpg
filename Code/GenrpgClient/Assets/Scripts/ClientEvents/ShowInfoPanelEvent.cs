using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.ClientEvents
{
    [MessagePackObject]
    public class ShowInfoPanelEvent
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        [Key(0)] public List<string> Lines { get; set; } = new List<string>();
    }
}
