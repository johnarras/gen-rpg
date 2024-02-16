using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Services;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Crawler.States
{
    public interface IStateHelper : ISetupDictionaryItem<ECrawlerStates>
    {
        UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token);
        bool IsTopLevelState();
    }
}
