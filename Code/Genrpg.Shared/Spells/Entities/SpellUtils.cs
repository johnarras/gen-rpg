using MessagePack;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Spells.Constants;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class SpellUtils
    {
        public static bool IsValidTarget(GameState gs, Unit target, long casterFactionId, long targetTypeId)
        {
            if (targetTypeId == TargetTypes.Enemy)
            {
                if (target.FactionTypeId != casterFactionId)
                {
                    return true;
                }
            }


            if (targetTypeId == TargetTypes.Ally)
            {
                if (target.FactionTypeId == casterFactionId)
                {
                    return true;
                }
            }

            return false;
        }
        public static float GetResendDelay(bool isInstant)
        {
            return isInstant ? SpellConstants.ResendInstantDelaySec : SpellConstants.ResendProjectileDelaySec;
        }
    }
}
