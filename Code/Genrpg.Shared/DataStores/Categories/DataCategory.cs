using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories
{
    [MessagePackObject]
    public class DataCategory : Attribute
    {
        [IgnoreMember] public string Category { get; set; }
    }

    public class DataCategoryTypes
    {
        public const string Default = "Default";
        public const string ContentData = "ContentData";
        public const string WorldData = "WorldData";
        public const string GameData = "GameData";
        public const string PlayerData = "PlayerData";
        public const string AccountData = "AccountData";

        public static readonly List<String> DataCategories = new List<string>() { AccountData, ContentData, WorldData, GameData, PlayerData };
    }
}
