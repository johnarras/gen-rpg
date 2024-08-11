using Genrpg.RequestServer.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.PlayerData.LoadUpdateHelpers
{
    public abstract class BaseCharacterLoadUpdater : ICharacterLoadUpdater
    {

        public Type GetKey() { return GetType(); }
        public virtual int Priority => 0;

        public abstract Task Update(WebContext context, Character ch);

    }
}
