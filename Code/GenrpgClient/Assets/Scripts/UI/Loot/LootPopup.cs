using System;
using System.Collections.Generic;
using UnityEngine;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Spawns.Entities;
using Cysharp.Threading.Tasks;
using System.Threading;

public class LootPopup : BaseScreen
{
    [SerializeField]
    private GameObject _itemAnchor;
    [SerializeField]
    private float _itemDelay = 0.5f;


    public override bool BlockMouse() { return false; }

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        List<SpawnResult> rewards = data as List<SpawnResult>;
        if (rewards == null || rewards.Count < 1)
        {
            StartClose();
            return;
        }

        ShowRewards(rewards, token).Forget();

        await UniTask.CompletedTask;
    }

    private async UniTask ShowRewards(List<SpawnResult> rewards, CancellationToken token)
    {
        if (rewards == null || rewards.Count < 1 || _itemAnchor == null)
        {
            StartClose();
            return;
        }

        foreach (SpawnResult rew in rewards)
        {
            InitItemIconData iid = new InitItemIconData()
            {
                Data = rew.Data as Item,
                entityTypeId = rew.EntityTypeId,
                entityId = rew.EntityId,
                quantity = rew.Quantity,
                level = rew.Level,
                quality = rew.QualityTypeId,
            };
            IconHelper.InitItemIcon(_gs, iid, _itemAnchor,_assetService, token);
        }
        
        while (true)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_itemDelay), cancellationToken: token);

            if (_itemAnchor.transform.childCount < 1)
            {
                break;
            }
            GameObject firstChild = _itemAnchor.transform.GetChild(0).gameObject;
            GameObject.Destroy(firstChild);
        }
        StartClose();
    }

}