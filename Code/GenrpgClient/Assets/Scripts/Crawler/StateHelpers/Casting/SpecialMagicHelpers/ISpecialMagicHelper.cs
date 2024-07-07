using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Casting.SpecialMagicHelpers
{
    public interface ISpecialMagicHelper : ISetupDictionaryItem<long>
    {
        Awaitable<CrawlerStateData> HandleEffect(SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect);
    }
}
