using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Users.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SearchService;

namespace Assets.Scripts.PlayerSearch
{
    public interface IPlayerSearchService : IInjectable
    {
        void AccountSearch(string accountId, Action<PublicAccount> handler, CancellationToken token);
        void UserSearch(string userId, Action<PublicUser> handler, CancellationToken token);
        void CharacterSearch(string charId, Action<PublicCharacter> handler, CancellationToken token);
    }
}
