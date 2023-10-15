using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Interfaces
{
    public interface IComplexCopy
    {
        void DeepCopyFrom(IComplexCopy from);
        object GetDeepCopyData();
    }
}
