using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GameSettings.Mappers
{
    public interface IGameSettingsMapper
    {
        Type GetServerType();
        bool SendToClient();
        ITopLevelSettings MapToApi(ITopLevelSettings settings);
    }
}
