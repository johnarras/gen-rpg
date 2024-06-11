using Assets.Scripts.Crawler.CrawlerStates;
using Genrpg.Shared.Interfaces;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.States
{
    public interface IStateHelper : ISetupDictionaryItem<ECrawlerStates>
    {
        Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token);
        bool IsTopLevelState();
    }
}
