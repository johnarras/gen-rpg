using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Maps;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;

namespace Genrpg.LoginServer.Core
{
    public class LoginGameState : ServerGameState
    {
        public User user { get; set; }
        public CoreCharacter coreCh { get; set; }
        public Character ch { get; set; }
        public MapStubList mapStubs { get; set; } = new MapStubList();
        public MyRandom rand { get; set; } = new MyRandom();
        public Dictionary<Type, IClientCommandHandler> commandHandlers = null;
        public Dictionary<Type, INoUserCommandHandler> noUserCommandHandlers = null;

        public List<ILoginResult> Results { get; set; } = new List<ILoginResult>();

        public LoginGameState(IServerConfig config) : base(config) { }

    }
}
