using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.GameSettings;

namespace Genrpg.Shared.Inventory.Settings.Slots
{
    /// <summary>
    /// List of equipment slots for characters
    /// </summary>
    [MessagePackObject]
    public class EquipSlot : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }


        /// <summary>
        /// Add a second slot to the given item type.
        /// </summary>
        [Key(6)] public long ParentEquipSlotId { get; set; }

        /// <summary>
        /// How much stats on generated items of this type are scaled.
        /// </summary>
        [Key(7)] public int StatPercent { get; set; }

        [Key(8)] public string Art { get; set; }

        [Key(9)] public bool Active { get; set; }
        public EquipSlot()
        {
            StatPercent = 100;
        }


    }
}
