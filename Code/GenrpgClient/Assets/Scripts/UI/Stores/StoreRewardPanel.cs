using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using System.Threading;

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


            IIndexedGameItem gameItem = (IIndexedGameItem)_entityService.Find(_gs.ch, spawnItem.EntityTypeId, spawnItem.EntityId);

            if (gameItem != null)
            {
                _assetService.LoadAtlasSpriteInto(AtlasNames.Icons, gameItem.Icon, RewardIcon, token);
            }

            _uiService.SetText(RewardQuantity, spawnItem.MinQuantity.ToString());

        }
    }
}
