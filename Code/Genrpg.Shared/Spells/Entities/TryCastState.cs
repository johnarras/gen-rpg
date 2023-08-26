using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Entities
{
    public enum TryCastState
    {
        Ok,
        IsPassive,
        CasterIncapacitated,
        CasterDead,
        CasterDeleted,
        CasterEvading,
        CasterBusy,
        DoNotKnowSpell,
        UnknownElement,
        UnknownSkill,
        TargetMissing,
        TargetDead,
        Evading,
        TargetDeleted,
        NotEnoughPower,
        OnCooldown,
        WrongTargetType,
        TargetTooFar,
        NoSpellData,
    }
}
