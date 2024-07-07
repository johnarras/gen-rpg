using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Crawler.States
{
    public enum ECrawlerStates
    {
        None,
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
