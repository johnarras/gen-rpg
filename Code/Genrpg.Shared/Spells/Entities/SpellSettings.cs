using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class SpellSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<SkillType> SkillTypes { get; set; }
        [Key(2)] public List<TargetType> TargetTypes { get; set; }
        [Key(3)] public List<SpellModifier> SpellModifiers { get; set; }
        [Key(4)] public List<Spell> Spells { get; set; }
        [Key(5)] public List<ProcType> ProcTypes { get; set; }
        [Key(6)] public List<OldSpell> OldSpells { get; set; }
        [Key(7)] public List<ElementType> ElementTypes { get; set; }

        public SkillType GetSkillType(long idkey) { return _lookup.Get<SkillType>(idkey); }
        public TargetType GetTargetType(long idkey) { return _lookup.Get<TargetType>(idkey); }
        public SpellModifier GetSpellModifier(long idkey) { return _lookup.Get<SpellModifier>(idkey); }
        public Spell GetSpell(long idkey) { return _lookup.Get<Spell>(idkey); }
        public ProcType GetProcType(long idkey) { return _lookup.Get<ProcType>(idkey); }
        public ElementType GetElementType(long idkey) { return _lookup.Get<ElementType>(idkey); }
    }
}
