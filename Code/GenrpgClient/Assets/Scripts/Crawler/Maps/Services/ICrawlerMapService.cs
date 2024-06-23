using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Services.CrawlerMaps
{
    public interface ICrawlerMapService : IInitializable
    {
        Awaitable EnterMap(PartyData partyData,  EnterCrawlerMapData mapData, CancellationToken token);
        Awaitable UpdateMovement(CancellationToken token);
        void ClearMovement();
        bool UpdatingMovement();
        string GetBuildingArtPrefix();
        ICrawlerMapTypeHelper GetHelper(ECrawlerMapTypes mapType);
        CrawlerMap Generate(CrawlerMapGenData genData);
    }
}
