using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Inventory.Entities
{
    /// <summary>
    /// List of equipment slots for characters
    /// </summary>
    [MessagePackObject]
    public class EquipSlot : IIndexedGameItem
    {
        public const int None = 0;
        public const int Helmet = 1;
        public const int Necklace = 2;
        public const int Shoulder = 3;
        public const int Chest = 4;
        public const int Cloak = 5;
        public const int Belt = 6;
        public const int Pants = 7;
        public const int Boots = 8;
        public const int Bracers = 9;
        public const int Gloves = 10;
        public const int Ring1 = 11;
        public const int Ring2 = 12;
        public const int Jewelry1 = 13;
        public const int Jewelry2 = 14;
        public const int MainHand = 15;
        public const int OffHand = 16;
        public const int Ranged = 17;


        /// <summary>
        /// Used to cap crazy equipment slots
        /// </summary>
        public const int Max = 20;

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }


        /// <summary>
        /// Add a second slot to the given item type.
        /// </summary>
        [Key(4)] public long ParentEquipSlotId { get; set; }

        /// <summary>
        /// How much stats on generated items of this type are scaled.
        /// </summary>
        [Key(5)] public int StatPercent { get; set; }

        [Key(6)] public string Art { get; set; }

        [Key(7)] public bool Active { get; set; }
        public EquipSlot()
        {
            StatPercent = 100;
        }


    }
}
