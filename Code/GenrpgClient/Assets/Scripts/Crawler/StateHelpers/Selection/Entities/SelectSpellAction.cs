using Genrpg.Shared.Crawler.Spells.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.StateHelpers.Selection.Entities
{
    public class SelectSpellAction
    {
        public CrawlerSpell Spell { get; set; }
        public SelectAction Action { get; set; }
        public long PowerCost { get; set; }
        public string PreviousError { get; set; }
    }
}
