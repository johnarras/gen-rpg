using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Spells.Casting;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Spells.Services
{
    public interface IServerSpellService : IInitializable
    {
        TryCastResult TryCast(IRandom rand, Unit unit, long spellId, string targetId, bool endOfCast);

        bool FullTryStartCast(IRandom rand, Unit unit, long spellId, string targetId);


        TargetCastState GetTargetState(IRandom rand, Spell spell, string targetId);

        void SendStopCast(IRandom rand, MapObject obj);

        void SendSpell(IRandom rand, Unit caster, TryCastResult result);

        void ResendSpell(IRandom rand, Unit caster, Unit target, SendSpell sendSpell);

        void OnSendSpell(IRandom rand, Unit origTarget, SendSpell sendSpell);

        void OnSpellHit(IRandom rand, SpellHit hit);

        void ShowFX(IRandom rand, string fromUnitId, string toUnitId, long elementTypeId, string fxName, float duration);

        void ShowProjectile(IRandom rand, Unit caster, Unit target, Spell spell, string fxName, float speed);

        void ShowCombatText(Unit unit, string txt, int combatTextColorId, bool isCrit = false);

        void ApplyOneEffect(IRandom rand, ActiveSpellEffect eff);
    }
}
