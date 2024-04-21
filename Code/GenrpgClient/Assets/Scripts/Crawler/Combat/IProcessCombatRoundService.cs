using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Services.Combat
{
    public interface IProcessCombatRoundCombatService : IInitializable
    {
        UniTask<bool> ProcessCombatRound(GameState gs, PartyData partyData, CancellationToken token);
    }
}
