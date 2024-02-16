using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UnitEffects.Services
{
    public class StatusEffectService : IStatusEffectService
    {
        public string ShowStatusEffects(GameState gs, Unit unit, bool showAbbreviations)
        {
            StringBuilder sb = new StringBuilder();
            if (unit == null)
            {
                return "";
            }

            IReadOnlyList<StatusEffect> effects = gs.data.Get<StatusEffectSettings>(unit).GetData();

            for (int i = 0; i < effects.Count; i++)
            {
                if (unit.StatusEffects.HasBit(i))
                {
                    if (showAbbreviations)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(' ');
                        }
                        sb.Append(effects[i].Abbrev);
                    }
                    else
                    {

                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append(effects[i].Name);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
