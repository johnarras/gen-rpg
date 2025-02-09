using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.BoardGame.Loading.Constants
{
    public class BoardMapConstants
    {
        public const int StartPos = MapConstants.TerrainPatchSize - 10 * CellSize;

        public const int TerrainBlockCount = 2;
        public const int CellSize = 4;
        public const int MaxDistanceFromPath = 4;
    }
}
