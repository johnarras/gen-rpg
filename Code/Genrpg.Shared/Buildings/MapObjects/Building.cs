using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapObjects.Entities;

namespace Genrpg.Shared.Buildings.MapObjects
{
    // MessagePackIgnore
    public class Building : MapObject
    {
        public Building(IRepositoryService repositoryService) : base(repositoryService) { }
    }
}
