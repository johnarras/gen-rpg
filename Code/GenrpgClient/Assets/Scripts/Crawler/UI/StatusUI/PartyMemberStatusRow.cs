
using Assets.Scripts.Assets.Textures;
using Assets.Scripts.Crawler.Combat;
using Assets.Scripts.Crawler.UI.StatusUI;
using Assets.Scripts.MVC;
using Assets.Scripts.UI.CombatTexts;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.StateHelpers.Exploring;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UI.Interfaces;
using Genrpg.Shared.UnitEffects.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.StatusUI
{


    public class PartyMemberStatusRow : BaseViewController<int,IView>
    {
        private IStatusEffectService _statusEffectService;
        private ICrawlerService _crawlerService;

        public IButton Button;
       
        private FastCombatTextUI _fastTextUI;

        private AnimatedSprite _portrait;

        private GameObject _root = null;

        private PartyMember _partyMember = null;

        private ProgressBar _healthBar;
        private ProgressBar _manaBar;

        private StatusEffectsUI _statusEffectsUI;

        private CombatEffectUI _combatEffectUI;

        private GText _nameText;
        private GText _classText;
        private GText _levelText;
        private GText _summonsText;


        public override async Task Init(int partyMemberIndex, IView view, CancellationToken token)
        {
            await base.Init(partyMemberIndex, view, token);
          

            Button = view.Get<IButton>("Button");
            _root = view.Get<GameObject>("Root");

            _portrait = view.Get<AnimatedSprite>("Portrait");

            _fastTextUI = view.Get<FastCombatTextUI>("FastCombatText");

            _nameText = _view.Get<GText>("Name");
            _levelText = _view.Get<GText>("Level");
            _classText = _view.Get<GText>("Class");
            _summonsText = _view.Get<GText>("Summons");

            _healthBar = _view.Get<ProgressBar>("HealthBar");
            _manaBar = _view.Get<ProgressBar>("ManaBar");

            _statusEffectsUI = _view.Get<StatusEffectsUI>("Status");
            _combatEffectUI = _view.Get<CombatEffectUI>("CombatEffects");

            _uiService.SetButton(Button, GetType().Name, ClickPartyMember);

            _updateService.AddUpdate(this, OnLateUpdate, UpdateTypes.Late, GetToken());
            UpdateData();
        }

        private void ClickPartyMember()
        {
            _partyMember = _crawlerService.GetParty().GetMemberInSlot(_model);

            if (_partyMember == null)
            {
                return;
            }

            _dispatcher.Dispatch(new CrawlerCharacterScreenData() { Unit = _partyMember });
        }

        private bool _needToUpdate = false;
        private long _nextElementTypeId = 0;
        public void UpdateData(long elementTypeId = 0)
        {
            _needToUpdate = true;
            if (_nextElementTypeId == 0)
            {
                _nextElementTypeId = elementTypeId;
            }
        }


        private void OnLateUpdate()
        {
            if (_needToUpdate)
            {
                UpdateDataInternal();
                _needToUpdate = false;

                if (_nextElementTypeId > 0)
                {
                    _combatEffectUI.OnShowCombatText(new Genrpg.Shared.Crawler.GameEvents.ShowCombatText() { ElementTypeId = _nextElementTypeId, Text = "", TextType = ECombatTextTypes.Damage });
                }
                _nextElementTypeId = 0;
              
            }
        }

        private void UpdateDataInternal()
        { 
            if (_model == 0)
            {
                return;
            }
            _partyMember = _crawlerService.GetParty().GetMemberInSlot(_model);

            if (_partyMember == null)
            {
                _clientEntityService.SetActive(_root, false);
                if (_fastTextUI != null)
                {
                    _fastTextUI.SetUnitId(null);
                }

                return;
            }
            else
            {
                if (_fastTextUI != null)
                {
                    _fastTextUI.SetUnitId(_partyMember.Id);
                }

                _clientEntityService.SetActive(_root, true);
                _uiService.SetText(_nameText, _partyMember.Name);
                _uiService.SetText(_levelText, _partyMember.Level.ToString());


                long currHp = _partyMember.Stats.Curr(StatTypes.Health);
                long maxHp = _partyMember.Stats.Max(StatTypes.Health);

                _healthBar.InitRange(0, _partyMember.Stats.Max(StatTypes.Health), _partyMember.Stats.Curr(StatTypes.Health));
                _manaBar.InitRange(0, _partyMember.Stats.Max(StatTypes.Mana), _partyMember.Stats.Curr(StatTypes.Mana));
                _statusEffectsUI.InitData(_partyMember);
                SetPortrait(_partyMember.PortraitName);

                RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);
                string classText = "";

                List<Role> roles = roleSettings.GetRoles(_partyMember.Roles);
                
                foreach (Role role in roles)
                {
                    if (!string.IsNullOrEmpty(classText))
                    {
                        classText += "/";
                    }

                    if (role.RoleCategoryId == RoleCategories.Class)
                    {
                        classText += role.Abbrev;
                    }
                }

                _uiService.SetText(_classText, classText);

                string summonText = "";

                if (_partyMember.Summons.Count> 0)
                {
                    summonText = _partyMember.Summons[0].Name;

                    if (_partyMember.Summons.Count > 1)
                    {
                        summonText += " (" + _partyMember.Summons.Count + ")";
                    }
                }
                _uiService.SetText(_summonsText, summonText);
            }
        }

        private void SetPortrait(string portraitName)
        {
            if (string.IsNullOrEmpty(portraitName))
            {
                portraitName = CrawlerClientConstants.DefaultWorldBG;
            }
            _portrait?.SetImage(portraitName);
        }

    }
}
