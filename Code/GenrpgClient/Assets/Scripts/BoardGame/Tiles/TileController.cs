
using Assets.Scripts.MVC;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.Tiles.Settings;
using Genrpg.Shared.UI.Interfaces;
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
        public int GridX { get; set; }
        public int GridZ { get; set; }
        public float XPos { get; set; }
        public float ZPos { get; set; }
    }

    public class TileController : BaseViewController<TileTypeWithIndex,IView>
    {
        public object PieceAnchor;
        public IAnimator Animator;
        public object TileMesh;
        public object[] PrizeAnchors;
        public bool[] ExtraData { get; set; } = new bool[ExtraTileSlots.Max];

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
                _clientEntityService.DestroyAllChildren(PrizeAnchors[ExtraTileSlots.Event]);
                ExtraData[ExtraTileSlots.Event] = false;
            }
            _clientEntityService.DestroyAllChildren(PrizeAnchors[ExtraTileSlots.Bonus]);
            ExtraData[ExtraTileSlots.Bonus] = false;
        }

        public override async Task Init(TileTypeWithIndex tileType, IView view, CancellationToken token)
        {
            await base.Init(tileType, view, token);
            PieceAnchor = _view.Get<object>("PieceAnchor");
            TileMesh = _view.Get<object>("TileMesh");
            Animator = _view.Get<IAnimator>("Animator");
            PrizeAnchors = new object[ExtraTileSlots.Max];
            PrizeAnchors[0] = _view.Get<object>("PassLootAnchor");
            PrizeAnchors[1] = _view.Get<object>("LandLootAnchor");
        }
    }
}
