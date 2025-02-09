using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Core.Constants
{

    public enum EGameModes
    {
        MMO = 0,
        Crawler = 1,
        BoardGame = 2,
    }

    public class GameModeUtils
    {
        public static bool IsPureClientMode(EGameModes mode)
        {
            return mode == EGameModes.Crawler;
        }
    }

}
