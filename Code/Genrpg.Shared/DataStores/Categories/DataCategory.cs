using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories
{
    [MessagePackObject]
    public class DataCategory : Attribute
    {
        public const string Default = "Default";
        public const string Content = "Content";
        public const string WorldData = "WorldData";
        public const string GameData = "GameData";
        public const string PlayerData = "PlayerData";

        [Key(0)] public string Category { get; set; }


    }
}
