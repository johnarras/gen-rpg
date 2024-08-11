using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.PlayerFiltering.Interfaces
{

    public interface IFilteredObjectContainer
    {

    }

    public interface IFilteredObject : IStringId
    {
        long Level { get; set; }
        DateTime CreationDate { get; set; }
        GameDataOverrideList DataOverrides { get; set; }
        string ClientVersion { get; set; }     
    }
}
