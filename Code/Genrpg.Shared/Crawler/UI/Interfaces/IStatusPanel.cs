using Genrpg.Shared.Crawler.Monsters.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.UI.Interfaces
{
    public interface IStatusPanel
    {
        void RefreshAll();

        void RefreshUnit(CrawlerUnit unit, string effect = null);
    }
}
