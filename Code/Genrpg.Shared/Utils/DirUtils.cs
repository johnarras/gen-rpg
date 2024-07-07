using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Utils
{
    public class DirUtils
    {
        public static int DirDeltaToAngle(int dx, int dy)
        {
            if (dy > 0)
            {
                return 0;
            }
            else if (dy < 0)
            {
                return 180;
            }
            else if (dx > 0)
            {
                return 90;
            }
            else if (dx < 0)
            {
                return 270;
            }
            return 0;
        }
    }
}
