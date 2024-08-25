using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Entities
{
    public class SwitchBoardModeArgs
    {
        public long BoardModeId { get; set; }
        public long Quantity { get; set; }
        public long ZoneTypeId { get; set; }
        public string OwnerId { get; set; }
    }
}
