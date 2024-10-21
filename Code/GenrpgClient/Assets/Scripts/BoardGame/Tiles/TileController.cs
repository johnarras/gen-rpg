
using Assets.Scripts.MVC;
using Assets.Scripts.ProcGen.Components;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.UI.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.BoardGame.Tiles
{

    public class TileTypeWithIndex
    {
        public TileType TileType { get; set; }
        public int Index { get; set; }
    }

    public class TileController : BaseViewController<TileTypeWithIndex,IView>
    {
        public MarkerPosition MarkerPos { get; set; }
        public object PieceAnchor;
        public IAnimator Animator;
        public object TileMesh;
        public object[] PrizeAnchors;
        public long[] PrizeIds { get; set; } = new long[BoardPrizeSlots.Max];

        public int GeTTileIndex()
        {
            return _model.Index; 
        }

        public long GetTileTypeId()
        {
            return _model.TileType.IdKey;
        }

        public virtual void ClearPrizes(bool landing)
        {
            if (landing)
            {
                _clientEntityService.DestroyAllChildren(PrizeAnchors[BoardPrizeSlots.Land]);
                PrizeIds[BoardPrizeSlots.Land] = 0;
            }
            _clientEntityService.DestroyAllChildren(PrizeAnchors[BoardPrizeSlots.Pass]);
            PrizeIds[BoardPrizeSlots.Pass] = 0;
        }

        public override async Task Init(TileTypeWithIndex tileType, IView view, CancellationToken token)
        {
            await base.Init(tileType, view, token);
            PieceAnchor = _view.Get<object>("PieceAnchor");
            TileMesh = _view.Get<object>("TileMesh");
            Animator = _view.Get<IAnimator>("Animator");
            PrizeAnchors = new object[BoardPrizeSlots.Max];
            PrizeAnchors[0] = _view.Get<object>("PassLootAnchor");
            PrizeAnchors[1] = _view.Get<object>("LandLootAnchor");
        }
    }
}
