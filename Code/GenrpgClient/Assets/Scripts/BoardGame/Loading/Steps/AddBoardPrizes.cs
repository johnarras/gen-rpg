using Assets.Scripts.BoardGame.Loading.Constants;
using Assets.Scripts.BoardGame.Services;
using Genrpg.Shared.BoardGame.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Loading.Steps
{
    public class AddBoardPrizes : BaseLoadBoardStep
    {
        private IBoardPrizeService _boardPrizeService;
        public override async Awaitable Execute(BoardData boardData, CancellationToken token)
        {
            await _boardPrizeService.UpdatePrizes(token);
        }

        public override ELoadBoardSteps GetKey() { return ELoadBoardSteps.AddBoardPrizes; }
    }
}
