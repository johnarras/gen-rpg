
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Users.PlayerData;
using System.Collections.Generic;

namespace Genrpg.Shared.Client.Core
{
    public interface IClientGameState : IGameState, IInjectable
    {
        User user { get; set; }
        Character ch { get; set; }
        List<CharacterStub> characterStubs { get; set; }
        List<MapStub> mapStubs { get; set; }
        string LoginServerURL { get; set; }
        InitialClientConfig GetConfig();
        EGameModes GameMode { get; set; }
        void UpdateUserFlags(int flag, bool val);
    }
}
