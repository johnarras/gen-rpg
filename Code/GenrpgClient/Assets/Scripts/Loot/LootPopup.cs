using System;
using System.Collections.Generic;
using UnityEngine;
using Genrpg.Shared.Inventory.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Rewards.Entities;
using System.Linq;

public class LootPopup : BaseScreen
{

    protected IIconService _iconService;
    public GameObject _itemAnchor;
    
    public float _itemDelay = 0.5f;


    public override bool BlockMouse() { return false; }

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        List<RewardList> rewardLists = data as List<RewardList>;
        if (rewardLists == null || rewardLists.Count < 1)
        {
            StartClose();
            return;
        }

        List<Reward> rewards = rewardLists.SelectMany(x=>x.Rewards).ToList();   
        

        _awaitableService.ForgetAwaitable(ShowRewards(rewards, token));


        await Task.CompletedTask;
    }

    private async Awaitable ShowRewards(List<Reward> rewards, CancellationToken token)
    {
        if (rewards == null || rewards.Count < 1 || _itemAnchor == null)
        {
            StartClose();
            return;
        }

        foreach (Reward rew in rewards)
        {
            InitItemIconData iid = new InitItemIconData()
            {
                Data = rew.ExtraData as Item,
                EntityTypeId = rew.EntityTypeId,
                EntityId = rew.EntityId,
                Quantity = rew.Quantity,
                Level = rew.Level,
                Quality = rew.QualityTypeId,
            };
            _iconService.InitItemIcon(iid, _itemAnchor,_assetService, token);
        }
        
        while (true)
        {
            await Awaitable.WaitForSecondsAsync(_itemDelay, cancellationToken: token);

            if (_itemAnchor.transform.childCount < 1)
            {
                break;
            }
            GameObject firstChild = _itemAnchor.transform.GetChild(0).gameObject;
            _clientEntityService.Destroy(firstChild);
        }
        StartClose();
    }

}