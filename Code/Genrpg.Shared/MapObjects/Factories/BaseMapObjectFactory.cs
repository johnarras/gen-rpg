
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Units.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.MapObjects.Factories
{
    public abstract class BaseMapObjectFactory : IMapObjectFactory
    {
        protected IUnitGenService _unitGenService;
        protected IGameData _gameData;
        protected IRepositoryService _repoService;
        protected IMapProvider _mapProvider;

        public virtual void Setup(IGameState gs)
        {
        }

        public abstract MapObject Create(IRandom rand, IMapSpawn spawn);
        public abstract long GetKey();

    }
}
