using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Constants
{
    public class ItemFlags
    {
        public const int PrimaryReagent = 1 << 0; // 1
        public const int FlagTwoHandedItem = 1 << 1; // 2
        public const int NoStack = 1 << 2; // 4
        public const int SkipScalingIconName = 1 << 3; // 8

    }
}
