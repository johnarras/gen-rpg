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
        Back,

        ExploreWorld,
        Error,

        WorldCast,
        SpecialSpellCast,

        UseItemExplore,
        UseItemCombat,

        PartyMember,

        SelectAlly,
        SelectEnemyGroup,
        SelectItem,
        SelectSpell,
        OnSelectSpell,      

        Vendor,

        TrainingMain,
        TrainingLevel,

        TempleBase,

        StartCombat,
        CombatFightRun,
        CombatPlayer,
        CombatConfirm,
        ProcessCombatRound,
        CombatDeath,
        CombatChest,
        CombatLoot,
        
    }
}
