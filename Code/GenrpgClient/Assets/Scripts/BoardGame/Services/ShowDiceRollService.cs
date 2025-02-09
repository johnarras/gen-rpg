using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.BoardGame.Players;
using Assets.Scripts.BoardGame.Tiles;
using Assets.Scripts.Lockouts.Constants;
using Assets.Scripts.Lockouts.Services;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.WebApi.RollDice;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Rewards.Services;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

namespace Assets.Scripts.BoardGame.Services
{

    public interface IShowDiceRollService : IInjectable, IInitOnResolve
    {
        Awaitable ShowDiceRoll(RollDiceResponse result, CancellationToken token);
    }

    public class ShowDiceRollService : IShowDiceRollService
    {

        private IBoardGameController _boardGameController;
        private IPlayerManager _playerManager;
        private IBoardPrizeService _boardPrizeService;
        private IClientGameState _gs;
        private IRewardService _rewardService;
        private IClientRandom _rand;
        private IClientEntityService _clientEntityService;
        private ILogService _logService;
        private ILockoutManager _lockoutManager;

        public void Init()
        {
        }

        public async Awaitable ShowDiceRoll(RollDiceResponse result, CancellationToken token)
        {
            _lockoutManager.AddLock(LockoutTypes.RollDice, RollDiceLocks.ShowRoll);
            GameObject player = _playerManager.GetPlayerGameObject();

            PlayerMarker marker = player.GetComponent<PlayerMarker>();

            BoardData currBoard = _gs.ch.Get<BoardData>();

            BoardData nextBoard = result.NextBoard;

            int prevTileIndex = result.InitialIndex;
            for (int step = 0; step < result.TilesIndexesReached.Count; step++)
            {
                int tileIndex = result.TilesIndexesReached[(int)step];


                Vector3 startPos = player.transform.position;
                TileController endTile = _boardGameController.GetTile(tileIndex);
                _clientEntityService.AddToParent(player, endTile.GetView());
                player.transform.position = startPos;


                GameObject endObj = endTile.PieceAnchor as GameObject;

                Vector3 endPos = endObj.transform.position;

                float groundDist = Vector3.Distance(startPos, endObj.transform.position);

                int totalFrames = (int)(groundDist / 0.3f);

                player.transform.LookAt(new Vector3(endPos.x,player.transform.position.y,endPos.z));

                for (int frame = 0; frame < totalFrames; frame++)
                {
                    float percent = (float)frame / totalFrames; 

                    Vector3 pos = startPos * (1-percent) + endPos * percent;

                    float extraHeight = percent * (1 - percent) * groundDist * 3;

                    player.transform.position = pos;
                    marker.View.transform.localPosition = new Vector3(0, extraHeight, 0);
                    await Awaitable.NextFrameAsync(token);

                    if (frame == totalFrames/2)
                    {
                        if (currBoard.Seed == nextBoard.Seed && prevTileIndex >= BoardGameConstants.FirstTileIndex)
                        {
                            if (currBoard.Tiles.Get(prevTileIndex) != nextBoard.Tiles.Get(prevTileIndex))
                            {
                                _boardGameController.SetTileTypeId(nextBoard.Tiles.Get(prevTileIndex), prevTileIndex, token);
                            }
                        }
                        prevTileIndex = tileIndex;
                    }

                }
                endTile.ClearPrizes(step == result.TilesIndexesReached.Count - 1);

                _clientEntityService.AddToParent(player, endTile.GetView());
                marker.View.transform.localPosition = Vector3.zero;

                RollStep currStep = result.Steps.FirstOrDefault(x => x.Step == step);

                if (currStep != null && currStep.Rewards.Count > 0)
                {
                    _rewardService.GiveRewards(_rand, _gs.ch, currStep.Rewards);
                }

            }


            _gs.ch.Set<BoardData>(result.NextBoard);
            await _boardPrizeService.UpdatePrizes(token);
            _lockoutManager.RemoveLock(LockoutTypes.RollDice, RollDiceLocks.ShowRoll);
        }
    }
}
