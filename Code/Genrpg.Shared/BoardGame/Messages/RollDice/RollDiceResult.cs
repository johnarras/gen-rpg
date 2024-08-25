using MessagePack;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.Rewards.Entities;
using System;
using System.Collections.Generic;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.BoardGame.Messages.RollDice
{

    [MessagePackObject]
    public class RollStep
    {
        [Key(0)] public int Step { get; set; }
        [Key(1)] public List<RewardList> Rewards { get; set; } = new List<RewardList>();
    }

    [MessagePackObject]
    public class RollDiceResult : IWebResult
    {

        [Key(0)] public int StartIndex { get; set; }
        [Key(1)] public int EndIndex { get; set; }

        [Key(2)] public int DiceCount { get; set; } = 2;
        [Key(3)] public int DiceSides { get; set; } = 6;
        [Key(4)] public bool FreeRolls { get; set; } = false;
        [Key(5)] public List<int> RollValues { get; set; } = new List<int>();
        [Key(6)] public List<int> TilesIndexesReached { get; set; } = new List<int>();
        [Key(7)] public int RollTotal { get; set; } = 0;

        [Key(8)] public List<RollStep> Steps { get; set; } = new List<RollStep>();

        [Key(9)] public BoardData NextBoard { get; set; }

    }
}
