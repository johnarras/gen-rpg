using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.BoardGame.Tiles;
using Genrpg.Shared.BoardGame.Messages.RollDice;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.Client.Core;
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

    public interface IShowDiceRollService : IInjectable, IInitOnResolve
    {
        Awaitable ShowDiceRoll(RollDiceResult result, CancellationToken token);
    }

    public class ShowDiceRollService : IShowDiceRollService
    {

        private IBoardGameController _boardGameController;
        private IPlayerManager _playerManager;
        private IBoardPrizeService _boardPrizeService;
        private IClientGameState _gs;
        public void Init()
        {
        }

        public async Awaitable ShowDiceRoll(RollDiceResult result, CancellationToken token)
        {

            GameObject player = _playerManager.GetPlayerGameObject();

            for (int tidx = 0; tidx < result.TilesIndexesReached.Count; tidx++)
            {

                int tileIndex = result.TilesIndexesReached[(int)tidx];
                Vector3 startPos = player.transform.position;

                TileArt endTile = _boardGameController.GetTile(tileIndex);

                float dist = Vector3.Distance(startPos, endTile.PieceAnchor.transform.position);

                int totalFrames = (int)(dist / 0.2f);

                for (int frame = 0; frame < totalFrames; frame++)
                {
                    player.transform.position = Vector3.Lerp(startPos, endTile.PieceAnchor.transform.position, 1.0f * frame / totalFrames);
                    player.transform.LookAt(endTile.PieceAnchor.transform.position);
                    await Awaitable.NextFrameAsync(token);
                    endTile.ClearPrizes(tidx == result.TilesIndexesReached.Count - 1);
                }

                player.transform.parent = endTile.transform;
                player.transform.position = endTile.transform.position;

            }


            _gs.ch.Set<BoardData>(result.NextBoard);
            await _boardPrizeService.UpdatePrizes(token);
        }
    }
}
