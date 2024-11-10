using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.UI.Interfaces;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Maps.Entities;

namespace Genrpg.Shared.Crawler.States.Services
{
    public interface ICrawlerService : IInitializable, ISpeedupListener
    {
        Task Init(PartyData partyData, CancellationToken token);
        void ChangeState(ECrawlerStates state, CancellationToken token, object extraData = null, ECrawlerStates returnState = ECrawlerStates.None);
        void ChangeState(CrawlerStateData currentState, CrawlerStateAction nextStateAction, CancellationToken token);
        Task OnFinishMove(bool movedPosition, CancellationToken token);
        PartyData GetParty();
        Task<PartyData> LoadParty(string filename = null);
        Task SaveGame();
        void ClearAllStates();
        CrawlerStateData PopState();
        CrawlerStateData GetTopLevelState();
        Task UpdateCrawlerUI();
        ECrawlerStates GetState();
        ForcedNextState TryGetNextForcedState(CrawlerMap map, int ex, int ez);
    }
}
