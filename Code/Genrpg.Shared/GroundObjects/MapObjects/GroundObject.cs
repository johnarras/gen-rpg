using Genrpg.Shared.MapObjects.Entities;

namespace Genrpg.Shared.GroundObjects.MapObjects
{
    // MessagePackIgnore
    public class GroundObject : MapObject
    {
        public override bool IsGroundObject() { return true; }
    }
}
