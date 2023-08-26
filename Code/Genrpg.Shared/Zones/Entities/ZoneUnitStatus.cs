using MessagePack;
namespace Genrpg.Shared.Zones.Entities
{
    [MessagePackObject]
    public class ZoneUnitStatus
    {
        [Key(0)] public long UnitTypeId { get; set; }

        /// <summary>
        /// Current population
        /// </summary>
        [Key(1)] public int Pop { get; set; }


        /// <summary>
        /// How many have been killed since last update
        /// </summary>
        [Key(2)] public int Killed { get; set; }



        [Key(3)] public string Prefix { get; set; }


    }
}
