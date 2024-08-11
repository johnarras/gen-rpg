using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.RequestServer.BoardGame.Helpers.TileTypeHelpers
{
    public abstract class BaseTileTypeHelper : ITileTypeHelper
    {
        public abstract long GetKey();
    }
}
