using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Interfaces
{
    public interface IUpdateData
    {
        DateTime CreateTime { get; set; }
        DateTime UpdateTime { get; set; }
    }
}
