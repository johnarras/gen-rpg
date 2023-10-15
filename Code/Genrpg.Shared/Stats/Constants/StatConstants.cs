using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Stats.Constants
{
    [MessagePackObject]
    public class StatConstants
    {
        public const long MinBaseStat = 10;

        public const float RegenTickSeconds = 1.0f;

        public const int PrimaryStatStart = 10; // Core stats used to derive other stats.
        public const int PrimaryStatEnd = 19; // Core stats used to derive other stats.


        public const int ScaleDownBegin = 30;
        public const int ScaleDownEnd = 39;

        public const int RatingPercentStart = 60;

        public const int MaxMutableStatTypeId = 9;

        public const float MinScaleUp = 0.10f;
        public const float MaxScaleDown = 5.0f;

    }
}
