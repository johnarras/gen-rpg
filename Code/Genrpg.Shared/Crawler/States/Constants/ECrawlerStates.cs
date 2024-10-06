using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.States.Constants
{
    public enum ECrawlerStates
    {
        None,
        DoNotChangeState,
        PopState,
        Lore,
        TavernMain,
        AddMember,
        RemoveMember,
        ChooseSex,
        ChooseRace,
        RollStats,
        ChooseClass,
        ChoosePortrait,
        ChooseName,
        DeleteMember,
        DeleteConfirm,
        DeleteYes,
        DeleteNo,

        Options,
        SaveGame,
        QuitGame,
        Help,

        ExploreWorld,
        MapExit,
        Error,
        GiveLoot,
        Riddle,

        WorldCast,
        SpecialSpellCast,

        SetWorldPortal,
        ReturnWorldPortal,
        TownPortal,
        TeleportPosition,
        JumpLength,
        PassWall,

        UseItemExplore,
        UseItemCombat,

        PartyMember,

        SelectAlly,
        SelectAllyTarget,
        SelectEnemyGroup,
        SelectItem,
        SelectSpell,
        OnSelectSpell,

        Vendor,

        TrainingMain,
        TrainingLevel,

        EnterHouse,

        TempleBase,

        StartCombat,
        CombatFightRun,
        CombatPlayer,
        CombatConfirm,
        ProcessCombatRound,
        CombatDeath,
    }
}
