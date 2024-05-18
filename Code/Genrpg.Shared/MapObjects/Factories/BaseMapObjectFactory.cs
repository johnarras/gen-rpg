
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

namespace Genrpg.Shared.MapObjects.Factories
{
    public abstract class BaseMapObjectFactory : IMapObjectFactory
    {
        protected IUnitGenService _unitGenService;
        protected IGameData _gameData;
        protected IRepositoryService _repoService;

        public virtual void Setup(GameState gs)
        {
        }

        public abstract MapObject Create(GameState gs, IMapSpawn spawn);
        public abstract long GetKey();

    }
}
