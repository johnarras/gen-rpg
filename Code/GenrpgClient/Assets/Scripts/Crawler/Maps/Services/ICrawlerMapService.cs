using Assets.Scripts.Crawler.Maps.Entities;

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

        bool UpdatingMovement();
    }
}
