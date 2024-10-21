using MessagePack;
using Genrpg.Shared.Crawler.Spells.Settings;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities
{
    [MessagePackObject]
    public class SelectSpellAction
    {
        [Key(0)] public CrawlerSpell Spell { get; set; }
        [Key(1)] public SelectAction Action { get; set; }
        [Key(2)] public long PowerCost { get; set; }
        [Key(3)] public string PreviousError { get; set; }
    }
}
