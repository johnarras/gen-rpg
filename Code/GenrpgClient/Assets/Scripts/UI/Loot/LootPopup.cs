using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Spawns.Entities;
using System.Threading.Tasks;
using System.Threading;

public class LootPopup : BaseScreen
{
    
    public GEntity _itemAnchor;
    
    public float _itemDelay = 0.5f;


    public override bool BlockMouse() { return false; }

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        List<SpawnResult> rewards = data as List<SpawnResult>;
        if (rewards == null || rewards.Count < 1)
        {
            StartClose();
            return;
        }

        TaskUtils.AddTask(ShowRewards(rewards, token));

        await Task.CompletedTask;
    }

    private async Task ShowRewards(List<SpawnResult> rewards, CancellationToken token)
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
            await Task.Delay(TimeSpan.FromSeconds(_itemDelay), cancellationToken: token);

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