using MessagePack;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Crafting.Entities
{
    [MessagePackObject]
    public class RecipeType : IIndexedGameItem
    {

        public const string RecipeItemName = "Recipe";

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string NameId { get; set; }
        [Key(3)] public string Desc { get; set; }
        [Key(4)] public string Icon { get; set; }
        [Key(5)] public long EntityId { get; set; }
        [Key(6)] public long EntityTypeId { get; set; }
        [Key(7)] public int MinQuantity { get; set; }
        [Key(8)] public int MaxQuantity { get; set; }
        [Key(9)] public string Art { get; set; }

        [Key(10)] public int AttPct { get; set; }
        [Key(11)] public int DefPct { get; set; }
        [Key(12)] public int OtherPct { get; set; }


        /// <summary>
        /// Use this for recipes that have a list of reagents rather than a choice.
        /// </summary>
        [Key(13)] public long CrafterTypeId { get; set; }


        [Key(14)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        [Key(15)] public int ReagentQuantity { get; set; }


        [Key(16)] public List<Reagent> Reagents { get; set; }

        public RecipeType()
        {
            Reagents = new List<Reagent>();
            MinQuantity = 1;
            MaxQuantity = 1;
            AttPct = 100;
            DefPct = 100;
            OtherPct = 100;
        }

    }
}
