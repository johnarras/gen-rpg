
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Services.Combat
{
    public interface IProcessCombatRoundCombatService : IInjectable
    {
        Awaitable<bool> ProcessCombatRound(PartyData partyData, CancellationToken token);
    }
}
