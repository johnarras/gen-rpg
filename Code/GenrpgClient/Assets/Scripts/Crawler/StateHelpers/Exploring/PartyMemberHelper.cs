﻿using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.UI.Screens.Characters;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Inventory.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UI.Screens.Constants;

namespace Assets.Scripts.Crawler.StateHelpers.Exploring
{
    public class PartyMemberHelper : BaseStateHelper
    {
        private IScreenService _screenService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.PartyMember; }

        public override async UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {

            PartyData partyData = _crawlerService.GetParty();
            PartyMember member = action.ExtraData as PartyMember;

            CrawlerStateData crawlerStateData = CreateStateData();
            CrawlerCharacterScreenData screenData = new CrawlerCharacterScreenData()
            {
                Unit = member,
                PrevState = currentData.Id,
            };

            InventoryData idata = member.Get<InventoryData>();

            idata.SetInvenEquip(partyData.Inventory, member.Equipment);

            _screenService.Open(gs, ScreenId.CrawlerCharacter, screenData);

            await UniTask.CompletedTask;
            return crawlerStateData;

            
        }
    }
}