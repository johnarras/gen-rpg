using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.UI.Interfaces;
using Genrpg.Shared.Interfaces;
using System.Threading;

namespace Assets.Scripts.Crawler.Services
{
    public interface ICrawlerService : ISetupService, ISpeedupListener
    {
        UniTask Init(CancellationToken token);
        void ChangeState(ECrawlerStates state, CancellationToken token, object extraData = null);
        void ChangeState(CrawlerStateData currentState, CrawlerStateAction nextStateAction, CancellationToken token);
        PartyData GetParty();
        UniTask LoadSaveGame();
        UniTask SaveGame();
    }
}
