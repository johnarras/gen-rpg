using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.MapServer.MapMessaging.Filters
{
    public class GroundObjectFilter : ObjectFilter
    {
        public override List<MapObject> Filter(IMapApiMessage message, List<MapObject> initialTargets)
        {
            return new List<MapObject>(initialTargets.Where(x => x.IsGroundObject()));
        }
    }
}
