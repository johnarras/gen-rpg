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

namespace Genrpg.MapServer.Spells
{
    public interface IServerSpellService : IInitializable
    {
        TryCastResult TryCast(GameState gs, Unit unit, long spellId, string targetId, bool endOfCast);

        bool FullTryStartCast(GameState gs, Unit unit, long spellId, string targetId);


        TargetCastState GetTargetState(GameState gs, Spell spell, string targetId);

        void SendStopCast(GameState gs, MapObject obj);

        void SendSpell(GameState gs, Unit caster, TryCastResult result);

        void ResendSpell(GameState gs, Unit caster, Unit target, SendSpell sendSpell);

        void OnSendSpell(GameState gs, Unit origTarget, SendSpell sendSpell);

        void OnSpellHit(GameState gs, SpellHit hit);

        void ShowFX(GameState gs, string fromUnitId, string toUnitId, long elementTypeId, string fxName, float duration);

        void ShowProjectile(GameState gs, Unit caster, Unit target, Spell spell, string fxName, float speed);

        void ShowCombatText(GameState gs, Unit unit, string txt, int combatTextColorId, bool isCrit = false);

        void ApplyOneEffect(GameState gs, ActiveSpellEffect eff);
    }
}
