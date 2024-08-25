using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.PlayMultiplier.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.PlayMultiplier.Services
{
    public interface ISharedPlayMultService : IInjectable
    {
        long GetMaxMult(IFilteredObject obj, long level, long energy);

        List<PlayMult> GetValidMults(IFilteredObject obj, long level, long energy);
    }
}
