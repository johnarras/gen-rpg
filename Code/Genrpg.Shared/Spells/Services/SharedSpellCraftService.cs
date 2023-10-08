
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.Spells.Entities;
using System;
using System.Linq;

namespace Genrpg.Shared.Spells.Services
{
    public class SharedSpellCraftService : ISharedSpellCraftService
    {

        private IReflectionService _reflectionService;

        public bool GenerateSpellData(GameState gs, Spell spell)
        {
            if (spell == null)
            {
                return false;
            }

            ElementType elemType = gs.data.GetGameData<ElementTypeSettings>(null).GetElementType(spell.ElementTypeId);
            SkillType skillType = gs.data.GetGameData<SkillTypeSettings>(null).GetSkillType(spell.SkillTypeId);

            if (elemType == null || skillType == null)
            {
                return false;
            }

            ElementSkill elemSkill = elemType.GetSkill(spell.SkillTypeId);
            if (elemSkill == null)
            {
                return false;
            }
            double cost = 1.0f;
            double scale = 1.0f;

            cost = skillType.PowerCost;
            scale = 1.0f;

            cost *= elemSkill.CostPct / 100.0f;
            scale *= elemSkill.ScalePct / 100.0f;

            foreach (SpellModifier mod in gs.data.GetGameData<SpellModifierSettings>(null).GetData())
            {
                if (mod.Values == null || mod.IsProcMod)
                {
                    continue;
                }

                int currValue = 0;

                try
                {
                    currValue = (int)_reflectionService.GetObjectValue(spell, mod.DataMemberName);
                }
                catch (Exception e)
                {
                    gs.logger.Exception(e, "SpellCraft " + mod.DataMemberName);
                    return false;
                }

                SpellModifierValue value = mod.Values.FirstOrDefault(x => x.Value == currValue);

                if (value == null)
                {
                    return false;
                }

                cost *= value.CostScale / 100.0f;

                if (mod.IdKey == SpellModifier.Scale)
                {
                    scale *= value.Value / 100.0f;
                }

            }

            spell.Cost = (int)cost;
            spell.FinalScale = (int)(100 * scale);


            if (string.IsNullOrEmpty(spell.Name))
            {
                spell.Name = elemType.Name + " " + skillType.Name;
            }

            return true;
        }
    }
}
