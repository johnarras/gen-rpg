using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Chat.Entities
{
    [MessagePackObject]
    public class ChatType : IIndexedGameItem
    {
        public const long None = 0;
        public const long Tell = 1;
        public const long Say = 2;
        public const long Yell = 3;
        public const long Zone = 4;
        public const long Map = 5;
        public const long World = 6;
        public const long Guild = 7;
        public const long Party = 8;
        public const long Raid = 9;

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Art { get; set; }
        [Key(5)] public string Color { get; set; }

    }
}
