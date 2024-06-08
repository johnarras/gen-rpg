using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapMessages.Interfaces;

namespace Genrpg.MapServer.MapMessaging.Filters
{
    public class ToPlayerFilter : ObjectFilter
    {
        public override List<MapObject> Filter(IMapApiMessage message, List<MapObject> initialTargets)
        {
            return new List<MapObject>(initialTargets.Where(x => x.IsUnit()));
        }
    }
}
