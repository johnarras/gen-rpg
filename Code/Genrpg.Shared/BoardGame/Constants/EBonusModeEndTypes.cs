using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Constants
{
    public enum EBonusModeEndTypes
    {
        None, // This has no special end condition except the 
        RollCount, // Use up a certain number of rolls
        HomeTile, // Lap to home tile
        StartTile, // Lap to starting tile
    };
}
