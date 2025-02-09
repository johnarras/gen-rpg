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
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.LoadSave.Constants;

namespace Genrpg.Shared.Crawler.States.Services
{
    public interface ICrawlerService : IInitializable, ISpeedupListener
    {
        CancellationToken GetToken();
        void ChangeState(ECrawlerStates state, CancellationToken token, object extraData = null, ECrawlerStates returnState = ECrawlerStates.None);
        void ChangeState(CrawlerStateData currentState, CrawlerStateAction nextStateAction, CancellationToken token);
        Task OnFinishMove(bool movedPosition, CancellationToken token);
        PartyData GetParty();
        Task SaveGame();
        PartyData LoadParty(long slotId = LoadSaveConstants.MinSlot);
        void ClearAllStates();
        bool ContinueGame();
        CrawlerStateData PopState();
        CrawlerStateData GetTopLevelState();
        Task UpdateCrawlerUI();
        ECrawlerStates GetState();
        ForcedNextState TryGetNextForcedState(CrawlerMap map, int ex, int ez);
        void NewGame(ECrawlerGameModes gameMode);
    }
}
