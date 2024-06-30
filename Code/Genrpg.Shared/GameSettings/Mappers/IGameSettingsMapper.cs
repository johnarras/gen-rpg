using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GameSettings.Mappers
{
    /// <summary>
    /// Use for mapping between client and server. Split from loader so client<->server and server<->database can vary independently
    /// </summary>
    public interface IGameSettingsMapper
    {
        Type GetServerType();
        Type GetClientType();
        bool SendToClient();
        ITopLevelSettings MapToApi(ITopLevelSettings settings);
    }
}
