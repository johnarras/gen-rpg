using Genrpg.Shared.MapMessages;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Maps.Messaging
{
    public class RemoveObjectFromGridCell : BaseMapMessage
    {
        public MapObjectGridData GridData { get; set; }
        public MapObjectGridItem GridItem { get; set; }
    }
}
