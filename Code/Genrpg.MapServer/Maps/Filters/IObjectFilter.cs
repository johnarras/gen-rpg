using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Maps.Filters
{
    public interface IObjectFilter : ISetupDictionaryItem<long>
    {
        List<MapObject> Filter(MapObject obj, List<MapObject> initialTargets);
    }
}
