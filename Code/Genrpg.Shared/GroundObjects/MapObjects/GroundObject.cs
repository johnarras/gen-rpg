using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapObjects.Entities;

namespace Genrpg.Shared.GroundObjects.MapObjects
{
    // MessagePackIgnore
    public class GroundObject : MapObject
    {
        public override bool IsGroundObject() { return true; }

        public GroundObject(IRepositoryService repoService): base(repoService) { }
    }
}
