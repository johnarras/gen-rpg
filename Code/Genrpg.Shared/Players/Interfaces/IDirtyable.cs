using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Players.Interfaces
{
    public interface IDirtyable
    {
        bool IsDirty();
        void SetDirty(bool val);
    }
}
