using Assets.Scripts.Crawler.Maps.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services
{
    public interface ICrawlerWorldService : IInjectable
    {

        Awaitable<CrawlerWorld> GetWorld(long worldId);
        void SetWorld(CrawlerWorld world);

        Awaitable SaveWorld(CrawlerWorld world);

        CrawlerWorld Generate(long worldId);

    }
}
