using Assets.Scripts.Assets.Textures;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.UnitEffects.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Combat
{
    public class CrawlerCombatUI : BaseBehaviour
    {
        private ICrawlerService _crawlerService;
        private ICrawlerMapService _crawlerMapService;

        public CrawlerGroupGrid AllyGrid;
        public CrawlerGroupGrid EnemyGrid;

        private bool _needToUpdateData = false;
        public void OnLateUpdate()
        {
            if (_needToUpdateData)
            {
                UpdateDataInternal();
                _needToUpdateData = false;
            }
        }

        public override void Init()
        {
            _updateService.AddUpdate(this, OnLateUpdate, UpdateTypes.Late, GetToken());  
        }

        public void InitData()
        {
        }

        public void UpdateData()
        {
            _needToUpdateData = true;
        }

        
        private void UpdateDataInternal()
        { 
            PartyData party = _crawlerService.GetParty();
            if (party.Combat == null)
            {
                AllyGrid.Clear();
                EnemyGrid.Clear();
            }
            else
            {
                AllyGrid.UpdateGroups(party.Combat.Allies);
                EnemyGrid.UpdateGroups(party.Combat.Enemies);
            }         
        }
    }
}
