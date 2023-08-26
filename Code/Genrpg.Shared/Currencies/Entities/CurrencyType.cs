using MessagePack;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.Currencies.Entities
{
    [MessagePackObject]
    public class CurrencyType : IIndexedGameItem
    {
        public const int None = 0;
        public const int Tokens = 1;
        public const int Money = 2;
        public const int Exp = 3;
        public const int Ability = 4;

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string PluralName { get; set; }
        [Key(3)] public string Desc { get; set; }
        [Key(4)] public string Icon { get; set; }
        [Key(5)] public string Art { get; set; }

    }
}
