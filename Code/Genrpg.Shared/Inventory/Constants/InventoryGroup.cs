using MessagePack;
namespace Genrpg.Shared.Inventory.Constants
{
    [MessagePackObject]
    public class InventoryGroup
    {
        public const int Equipment = 1 << 0;
        public const int Reagents = 1 << 1;

        public const int All = Equipment | Reagents;

    }
}
