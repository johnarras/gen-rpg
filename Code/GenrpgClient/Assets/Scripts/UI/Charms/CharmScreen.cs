﻿using Cysharp.Threading.Tasks;
using Genrpg.Shared.Charms.PlayerData;
using System.Collections.Generic;
using System.Threading;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.UI.Charms
{

    public class CharmScreen : BaseScreen
    {
        const string CharmRowPrefabName = "CharmRow";
        public GButton CloseButton;
        public GEntity RowParent;


        protected override async UniTask OnStartOpen(object data, CancellationToken token)
        {
            PlayerCharmData charmData = _gs.ch.Get<PlayerCharmData>();

            foreach (PlayerCharm status in charmData.GetData())
            {
                _assetService.LoadAssetInto(_gs, RowParent, AssetCategoryNames.UI, CharmRowPrefabName, OnLoadStatusRow, status, token, "Charms");
            }


            await UniTask.CompletedTask;
        }

        private void OnLoadStatusRow (UnityGameState gs, string url, object obj, object data, CancellationToken token)
        {
            GEntity go = obj as GEntity;

            if (go == null)
            {
                return;
            }

            CharmRow row = go.GetComponent<CharmRow>();
            if (row == null)
            {
                GEntityUtils.Destroy(go);
                return;
            }

            row.Init(data as PlayerCharm);
        }
    }
}
