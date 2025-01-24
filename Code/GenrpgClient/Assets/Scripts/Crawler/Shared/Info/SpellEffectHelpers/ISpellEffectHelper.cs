using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.EffectHelpers
{
    public interface ISpellEffectHelper : ISetupDictionaryItem<long>
    {
        string ShowEffectInfo(CrawlerSpell spell, CrawlerSpellEffect effect);
    }
}
