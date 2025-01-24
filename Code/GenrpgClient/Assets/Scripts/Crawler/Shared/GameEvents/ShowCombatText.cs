using MessagePack;
using Genrpg.Shared.Crawler.Combat.Constants;

namespace Genrpg.Shared.Crawler.GameEvents
{

    [MessagePackObject]
    public class ShowCombatText
    {
        [Key(0)] public string UnitId { get; set; }
        [Key(1)] public string GroupId { get; set; }
        [Key(2)] public string Text { get; set; }
        [Key(3)] public ECombatTextTypes TextType { get; set; }
    }
}
