
using Assets.Scripts.ProcGen.Components;
using Genrpg.Shared.BoardGame.Constants;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Tiles
{
    public class TileArt : BaseBehaviour
    {
        public long TileTypeId;
        public MarkerPosition MarkerPos { get; set; }
        public GameObject PieceAnchor;
        public Animator Animator;
        public MeshFilter TileMesh;
        public GameObject[] PrizeAnchors;
        public long[] PrizeIds { get; set; } = new long[BoardPrizeSlots.Max];


        public void ClearPrizes(bool landing)
        {
            if (landing)
            {
                _gameObjectService.DestroyAllChildren(PrizeAnchors[BoardPrizeSlots.Land]);
                PrizeIds[BoardPrizeSlots.Land] = 0;
            }
            _gameObjectService.DestroyAllChildren(PrizeAnchors[BoardPrizeSlots.Pass]);
            PrizeIds[BoardPrizeSlots.Pass] = 0;
        }
    }
}
