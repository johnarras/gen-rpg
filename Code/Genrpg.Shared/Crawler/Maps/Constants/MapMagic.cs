using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Maps.Constants
{
    public class MapMagic
    {
        public const int Darkness = 1 << 0;
        public const int Spinner = 1 << 1;
        public const int Drain = 1 << 2;
        public const int NoMagic = 1 << 3;
        public const int Peaceful = 1 << 4;

        public static readonly char[] Letters = { 'D', 'S', 'H', 'A' };
    }
}
