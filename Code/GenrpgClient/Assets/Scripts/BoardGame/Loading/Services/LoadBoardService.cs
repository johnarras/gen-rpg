using Assets.Scripts.BoardGame.Loading;
using Assets.Scripts.BoardGame.Loading.Constants;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Services
{

    public interface ILoadBoardService : IInjectable
    {
        Awaitable LoadBoard(BoardData boardData, CancellationToken token);
    }


    public class LoadBoardService : ILoadBoardService
    {

        private OrderedSetupDictionaryContainer<ELoadBoardSteps, ILoadBoardStep> _steps = new OrderedSetupDictionaryContainer<ELoadBoardSteps, ILoadBoardStep>();

        public async Awaitable LoadBoard(BoardData boardData, CancellationToken token)
        {
            foreach (ILoadBoardStep step in _steps.OrderedItems())
            {
                await step.Execute(boardData, token);
            }

            await Task.CompletedTask;
        }
    }
}
