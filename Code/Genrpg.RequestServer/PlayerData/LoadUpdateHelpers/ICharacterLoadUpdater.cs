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
    public interface ICharacterLoadUpdater : IOrderedSetupDictionaryItem<Type>
    {
        int Order { get; }
        Task Update(WebContext context, Character ch);
    }
}
