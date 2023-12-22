using MessagePack;
namespace Genrpg.Shared.Spells.Settings.Elements
{
    [MessagePackObject]
    public class ElementSkill
    {
        [Key(0)] public long SkillTypeId { get; set; }
        /// <summary>
        /// Percent cost to use this skill with this element. 100 = normal
        /// </summary>
        [Key(1)] public int CostPct { get; set; }
        /// <summary>
        /// Percent damage/healing/statmodifier to use this skill with this element. 100 = normal
        /// </summary>
        [Key(2)] public int ScalePct { get; set; }

        [Key(3)] public long OverrideEntityTypeId { get; set; }
        [Key(4)] public long OverrideEntityId { get; set; }

        public ElementSkill()
        {
            CostPct = 100;
            ScalePct = 100;
        }
    }
}
