
using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.Client.Assets.Constants;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Charms
{

    public class CharmScreen : BaseScreen
    {
        const string CharmRowPrefabName = "CharmRow";
        public GameObject RowParent;


        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            PlayerCharmData charmData = _gs.ch.Get<PlayerCharmData>();

            foreach (PlayerCharm status in charmData.GetData())
            {
                _assetService.LoadAssetInto(RowParent, AssetCategoryNames.UI, CharmRowPrefabName, OnLoadStatusRow, status, token, "Charms");
            }

            await Task.CompletedTask;
        }

        private void OnLoadStatusRow (object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            CharmRow row = go.GetComponent<CharmRow>();
            if (row == null)
            {
                _gameObjectService.Destroy(go);
                return;
            }

            row.Init(data as PlayerCharm);
        }
    }
}
