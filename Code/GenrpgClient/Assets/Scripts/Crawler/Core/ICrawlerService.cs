using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.UI.Interfaces;
using Genrpg.Shared.Interfaces;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Services
{
    public interface ICrawlerService : IInitializable, ISpeedupListener
    {
        Awaitable Init(PartyData partyData, CancellationToken token);
        void ChangeState(ECrawlerStates state, CancellationToken token, object extraData = null);
        void ChangeState(CrawlerStateData currentState, CrawlerStateAction nextStateAction, CancellationToken token);
        Awaitable OnFinishMove(bool movedPosition, CancellationToken token);
        PartyData GetParty();
        Awaitable<PartyData> LoadParty(string filename = null);
        Awaitable SaveGame();
        void ClearAllStates();
        CrawlerStateData PopState();
        CrawlerStateData GetTopLevelState();
        Awaitable UpdateCrawlerUI();
        void CreateSpline();
    }
}
