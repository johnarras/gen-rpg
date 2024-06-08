using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.MapObjects.Factories
{
    public interface IMapObjectFactory : ISetupDictionaryItem<long>
    {
        void Setup(IGameState gs);
        MapObject Create(IRandom rand, IMapSpawn spawn);

    }
}
