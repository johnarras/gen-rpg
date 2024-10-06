using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.BoardGame.Loading.Constants
{
    public enum ELoadBoardSteps
    {
        CleanOldBoard,
        CreateTerrain,
        LoadTiles,
        LoadPlayer,
        AddTrees,
        PaintTerrain,
        AddBoardPrizes,
    }
}
