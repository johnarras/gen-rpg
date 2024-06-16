using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Spawns.Entities;

using System.Threading;
using UnityEngine;
using System.Threading.Tasks;

public class LootPopup : BaseScreen
{
    
    public GEntity _itemAnchor;
    
    public float _itemDelay = 0.5f;


    public override bool BlockMouse() { return false; }

    protected override async Awaitable OnStartOpen(object data, CancellationToken token)
    {
        List<SpawnResult> rewards = data as List<SpawnResult>;
        if (rewards == null || rewards.Count < 1)
        {
            StartClose();
            return;
        }

        ShowRewards(rewards, token);


        await Task.CompletedTask;
    }

    private async Awaitable ShowRewards(List<SpawnResult> rewards, CancellationToken token)
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
                EntityTypeId = rew.EntityTypeId,
                EntityId = rew.EntityId,
                Quantity = rew.Quantity,
                Level = rew.Level,
                Quality = rew.QualityTypeId,
            };
            IconHelper.InitItemIcon(iid, _itemAnchor,_assetService, token);
        }
        
        while (true)
        {
            await Awaitable.WaitForSecondsAsync(_itemDelay, cancellationToken: token);

            if (_itemAnchor.transform().childCount < 1)
            {
                break;
            }
            GEntity firstChild = _itemAnchor.transform().GetChild(0).entity();
            GEntityUtils.Destroy(firstChild);
        }
        StartClose();
    }

}