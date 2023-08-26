using MessagePack;
namespace Genrpg.Shared.ProcGen.Entities
{
    /// <summary>
    /// Set up what this creature likes to eat. It can be plants or animals or both.
    /// </summary>

    [MessagePackObject]
    public class MonsterFood
    {
        /// <summary>
        /// What entity type the desired food is
        /// </summary>
        [Key(0)] public long FoodEntityTypeId { get; set; }

        /// <summary>
        /// What the entity key is for this food type.
        /// </summary>
        [Key(1)] public int FoodEntityId { get; set; }

    }
}
