using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GameSettings.Entities
{
    [MessagePackObject]
    public class GameDataOverrideList
    {
        [Key(0)] public List<GameDataOverrideItem> Items { get; set; } = new List<GameDataOverrideItem>();      
    }
}
