using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.UnitEffects.Services
{
    public class StatusEffectService : IStatusEffectService
    {
        private IGameData _gameData;
        public async Task Initialize(GameState gs, CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        public string ShowStatusEffects(GameState gs, Unit unit, bool showAbbreviations)
        {
            StringBuilder sb = new StringBuilder();
            if (unit == null)
            {
                return "";
            }

            IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(unit).GetData();

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
