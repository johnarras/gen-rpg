using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Players.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapObjects.Interfaces
{
    public interface IMapObject : IDirtyable, IFilteredObject
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
    }
}
