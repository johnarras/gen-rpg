using Assets.Scripts.Crawler.Maps.Entities;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Services.CrawlerMaps
{
    public interface ICrawlerMapService : IInitializable
    {
        UniTask EnterMap(PartyData partyData,  EnterCrawlerMapData mapData, CancellationToken token);
        UniTask UpdateMovement(CancellationToken token);

        bool UpdatingMovement();
    }
}
