using MessagePack;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.Rewards.Entities;
using System;
using System.Collections.Generic;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.BoardGame.Constants;

namespace Genrpg.Shared.BoardGame.WebApi.RollDice
{

    [MessagePackObject]
    public class RollStep
    {
        [Key(0)] public int Step { get; set; }
        [Key(1)] public List<RewardList> Rewards { get; set; } = new List<RewardList>();
    }

    [MessagePackObject]
    public class RollDiceResponse : IWebResponse
    {

        [Key(0)] public long DiceRollResult { get; set; } = DiceRollResults.Ok;
        [Key(1)] public int StartIndexReached { get; set; }
        [Key(2)] public int EndIndexReached { get; set; }

        [Key(3)] public int DiceCount { get; set; } = 2;
        [Key(4)] public int DiceSides { get; set; } = 6;
        [Key(5)] public bool FreeRolls { get; set; } = false;
        [Key(6)] public List<int> RollValues { get; set; } = new List<int>();
        [Key(7)] public List<int> TilesIndexesReached { get; set; } = new List<int>();
        [Key(8)] public int RollTotal { get; set; } = 0;

        [Key(9)] public List<RollStep> Steps { get; set; } = new List<RollStep>();

        [Key(10)] public BoardData NextBoard { get; set; }

        [Key(11)] public long DiceCost { get; set; }

        [Key(12)] public int InitialIndex { get; set; }

    }
}
