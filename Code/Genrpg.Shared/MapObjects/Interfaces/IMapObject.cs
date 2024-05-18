using Genrpg.Shared.PlayerFiltering.Interfaces;

namespace Genrpg.Shared.MapObjects.Interfaces
{
    public interface IMapObject : IFilteredObject
    {
        string Id { get; set; }
        string Name { get; set; }
        long EntityTypeId { get; set; }
        long EntityId { get; set; }
        float X { get; set; }
        float Y { get; set; }
        float Z { get; set; }
        float Rot { get; set; }
        float Speed { get; set; }
        long ZoneId { get; set; }
        string LocationId { get; set; }
        string LocationPlaceId { get; set; }
        long AddonBits { get; set; }
        bool IsDirty();
        void SetDirty(bool val);
    }
}
