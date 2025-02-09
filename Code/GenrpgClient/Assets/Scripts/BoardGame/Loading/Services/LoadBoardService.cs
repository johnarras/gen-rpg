using Assets.Scripts.BoardGame.Loading;
using Assets.Scripts.BoardGame.Loading.Constants;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Services;
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

        private IScreenService _screenService;
        private ILogService _logService;

        private OrderedSetupDictionaryContainer<ELoadBoardSteps, ILoadBoardStep> _steps = new OrderedSetupDictionaryContainer<ELoadBoardSteps, ILoadBoardStep>();

        public async Awaitable LoadBoard(BoardData boardData, CancellationToken token)
        {

           
            _screenService.Open(ScreenId.Loading);
            try
            {


                foreach (ILoadBoardStep step in _steps.OrderedItems())
                {
                    await step.Execute(boardData, token);
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "LoadBoardMap");
            }

            await Awaitable.NextFrameAsync(token);
            _screenService.Close(ScreenId.Loading);
        }
    }
}
