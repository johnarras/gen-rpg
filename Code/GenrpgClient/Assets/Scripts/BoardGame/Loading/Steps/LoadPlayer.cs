using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.BoardGame.Loading.Constants;
using Assets.Scripts.BoardGame.Players;
using Assets.Scripts.BoardGame.Tiles;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Users.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Loading.Steps
{
    public class LoadPlayer : BaseLoadBoardStep
    {
        private IPlayerManager _playerManager;
        public override async Awaitable Execute(BoardData boardData, CancellationToken token)
        {
            CoreUserData userData = _gs.ch.Get<CoreUserData>();

            Marker marker = _gameData.Get<MarkerSettings>(_gs.ch).Get(userData.MarkerId);

            TileController tile = _boardGameController.GetTile(boardData.TileIndex);

            _assetService.LoadAssetInto(tile.PieceAnchor, AssetCategoryNames.Markers, marker.Art + userData.MarkerTier, OnLoadMarker, null, token);
            await Task.CompletedTask;
        }

        public override ELoadBoardSteps GetKey() { return ELoadBoardSteps.LoadPlayer; }

        private void OnLoadMarker(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            GameObject parent = new GameObject() { name = "Player" };
            _clientEntityService.AddToParent(parent, go.transform.parent.gameObject);
            PlayerMarker marker = parent.AddComponent<PlayerMarker>();
            marker.View = go;
            _clientEntityService.AddToParent(go, parent);
            _playerManager.SetEntity(parent);


        }
    }
}
