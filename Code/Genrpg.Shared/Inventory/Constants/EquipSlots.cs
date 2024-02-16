using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Genrpg.Shared.Inventory.Constants
{
    public class EquipSlots
    {
        public const long None = 0;
        public const long Helmet = 1;
        public const long Necklace = 2;
        public const long Shoulder = 3;
        public const long Chest = 4;
        public const long Cloak = 5;
        public const long Belt = 6;
        public const long Pants = 7;
        public const long Boots = 8;
        public const long Bracers = 9;
        public const long Gloves = 10;
        public const long Ring1 = 11;
        public const long Ring2 = 12;
        public const long Jewelry1 = 13;
        public const long Jewelry2 = 14;
        public const long MainHand = 15;
        public const long OffHand = 16;
        public const long Ranged = 17;
        public const long PoisonVial = 18;
        public const long Quiver = 19;


        /// <summary>
        /// Used to cap crazy equipment slots
        /// </summary>
        public const long Max = 30;



        public static bool IsWeapon(long equipSlotId)
        {
            return equipSlotId == MainHand || equipSlotId == Ranged;
        }

        public static bool IsArmor(long equipSlotId)
        {
            return equipSlotId > 0 && !IsWeapon(equipSlotId);
        }
    }
}
