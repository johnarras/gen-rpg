using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Configs.Interfaces
{
    public interface IConnectionConfig
    {
        string GetConnectionString(string key);
        Dictionary<string, string> GetConnectionStrings();
    }
}
