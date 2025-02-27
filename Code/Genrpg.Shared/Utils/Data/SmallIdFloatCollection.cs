using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Utils.Data
{
    public class SmallIdFloatCollection : BaseSmallIdCollection<float>
    {
        protected override float InternalAdd(float first, float second)
        {
            return first + second;
        }

        protected override bool IsDefault(float t)
        {
            return t == 0;
        }
    }
}
