﻿using Genrpg.ServerShared.Core;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapMods.Helpers;
using Genrpg.Shared.MapMods.MapObjectAddons;
using Genrpg.Shared.MapMods.MapObjects;
using Genrpg.Shared.Reflection.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.MapServer.MapMods.Services
{
    public class MapModService : IMapModService
    {
        private IReflectionService _reflectionService = null;
        private Dictionary<long, IMapModEffectHelper> _effectHelpers = null;
        public async Task Setup(GameState gs, CancellationToken token)
        {
            _effectHelpers = _reflectionService.SetupDictionary<long, IMapModEffectHelper>(gs);
        }

        protected IMapModEffectHelper GetHelper(long mapModEffectTypeId)
        {
            if (_effectHelpers.TryGetValue(mapModEffectTypeId, out IMapModEffectHelper helper))
            {
                return helper;
            }
            return null;
        }

        public void Process(ServerGameState gs, MapMod mapMod)
        {
            MapModAddon addon = mapMod.GetAddon<MapModAddon>();
            if (addon == null)
            {
                return;
            }

            foreach (MapModEffect effect in addon.Effects)
            {
                IMapModEffectHelper helper = GetHelper(effect.MapModEffectTypeId);
                if (helper != null)
                {
                    helper.Process(gs, mapMod, addon, effect);
                }
            }
        }
    }
}
