using MessagePack;
namespace Genrpg.Shared.ProcGen.Entities
{
    /// <summary>
    /// This contains information about this location.
    /// The faction information is separated, I guess
    /// there might be a chance for multiple factions
    /// to be here or something. Not sure, but
    /// it should be separate
    /// 
    /// </summary>
    [MessagePackObject]
    public class Location
    {
        /// <summary>
        /// Location id in the zone
        /// </summary>
        [Key(0)] public long Id { get; set; }

        [Key(1)] public long ZoneId { get; set; }
        /// <summary>
        ///  What kind of location this is
        /// </summary>
        [Key(2)] public long LocationTypeId { get; set; }
        /// <summary>
        /// Name of the location
        /// </summary>
        [Key(3)] public string Name { get; set; }
        /// <summary>
        /// Description of the location
        /// </summary>
        [Key(4)] public string Description { get; set; }

        /// <summary>
        /// Location xpos on map
        /// </summary>
        [Key(5)] public int CenterX { get; set; }
        /// <summary>
        /// Location ypos on map
        /// </summary>
        [Key(6)] public int CenterZ { get; set; }

        /// <summary>
        /// MyRandom seed for generating content
        /// </summary>
        [Key(7)] public long Seed { get; set; }

        /// <summary>
        /// XSize in units
        /// </summary>
        [Key(8)] public int XSize { get; set; }
        /// <summary>
        /// YSize in units
        /// </summary>
        [Key(9)] public int ZSize { get; set; }



        [Key(10)] public string ExtraZone { get; set; }


        public bool IsRectangular()
        {
            return false;
        }


    }
}
