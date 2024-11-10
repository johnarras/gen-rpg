using Genrpg.Shared.GameSettings;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.PlayMultiplier.Settings;
using Genrpg.Shared.Users.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.PlayMultiplier.Services
{
    public class SharedPlayMultService : ISharedPlayMultService
    {
        private IGameData _gameData = null;
        public long GetMaxMult(IFilteredObject obj, long level, long energy)
        {
            return GetValidMults(obj, level, energy).Last().Mult;
        }

        public List<PlayMult> GetValidMults(IFilteredObject obj, long level, long energy)
        {
            return _gameData.Get<PlayMultSettings>(obj).GetData().
                Where(x => x.MinLevel > 0 && x.MinLevel <= level ||
                x.MinEnergy > 0 && x.MinEnergy <= energy).OrderBy(X => X.Mult).ToList();
        }
    }
}
