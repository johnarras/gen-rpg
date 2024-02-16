using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.MapMessaging.Filters
{
    public abstract class ObjectFilter
    {
        public abstract List<MapObject> Filter(GameState gs, IMapApiMessage message, List<MapObject> initialTargets);
    }
}
