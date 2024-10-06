using Genrpg.Shared.Crawler.Spells.Settings;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities
{
    public class SelectSpellAction
    {
        public CrawlerSpell Spell { get; set; }
        public SelectAction Action { get; set; }
        public long PowerCost { get; set; }
        public string PreviousError { get; set; }
    }
}
