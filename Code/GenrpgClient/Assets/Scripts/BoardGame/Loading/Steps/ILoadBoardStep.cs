using Assets.Scripts.BoardGame.Loading.Constants;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Loading
{
    public interface ILoadBoardStep : IOrderedSetupDictionaryItem<ELoadBoardSteps>
    {
        Awaitable Execute(BoardData boardData, CancellationToken token);
    }
}
