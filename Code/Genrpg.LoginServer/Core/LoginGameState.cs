using Genrpg.LoginServer.CommandHandlers;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Users.Entities;
using System;
using System.Collections.Generic;

namespace Genrpg.LoginServer.Core
{
    public class LoginGameState : ServerGameState
    {
        public User user { get; set; }
        public Character ch { get; set; }
        public List<MapStub> mapStubs = new List<MapStub>();
        public Dictionary<Type, ILoginCommandHandler> commandHandlers = null;

        public List<ILoginResult> Results { get; set; } = new List<ILoginResult>();

    }
}
