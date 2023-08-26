using MessagePack;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.UserCoins.Entities
{
    [MessagePackObject]
    public class UserCoinType : IIndexedGameItem
    {
        public const int None = 0;
        public const int Doubloons = 1;


        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string PluralName { get; set; }
        [Key(3)] public string Desc { get; set; }
        [Key(4)] public string Icon { get; set; }
        [Key(5)] public string Art { get; set; }

    }
}
