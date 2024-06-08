using Assets.Scripts.Atlas.Constants;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Stores
{
    public class StoreRewardPanel : BaseBehaviour
    {
        private IEntityService _entityService = null;
        public GImage RewardIcon;
        public GText RewardQuantity;

        private SpawnItem _spawnItem;
        public void Init(SpawnItem spawnItem, CancellationToken token)
        {
            _spawnItem = spawnItem;


            IIndexedGameItem gameItem = _entityService.Find(_gs.ch, spawnItem.EntityTypeId, spawnItem.EntityId);

            if (gameItem != null)
            {
                _assetService.LoadAtlasSpriteInto(AtlasNames.Icons, gameItem.Icon, RewardIcon, token);
            }

            _uIInitializable.SetText(RewardQuantity, spawnItem.MinQuantity.ToString());

        }
    }
}
