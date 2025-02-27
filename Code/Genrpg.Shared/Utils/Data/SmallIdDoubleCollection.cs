using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Utils.Data
{
    public class SmallIdDoubleCollection : BaseSmallIdCollection<double>
    {
        protected override double InternalAdd(double first, double second)
        {
            return first + second;
        }

        protected override bool IsDefault(double t)
        {
            return t == 0;
        }
    }
}
