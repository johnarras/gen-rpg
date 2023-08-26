using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Core.Entities;

namespace Genrpg.Shared.MapObjects.Factories
{
    public interface IMapObjectFactory : ISetupDictionaryItem<long>
    {
        void Setup(GameState gs);
        MapObject Create(GameState gs, IMapSpawn spawn);

    }
}
