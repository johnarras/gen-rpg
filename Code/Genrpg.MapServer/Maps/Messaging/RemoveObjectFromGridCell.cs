using Genrpg.Shared.MapMessages;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Maps.Messaging
{
    public sealed class RemoveObjectFromGridCell : BaseMapMessage
    {
        public MapObjectGridData Data;
        public MapObjectGridItem Item;

    }
}