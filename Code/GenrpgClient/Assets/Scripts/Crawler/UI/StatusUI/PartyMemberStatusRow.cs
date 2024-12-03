
using Assets.Scripts.Assets.Textures;
using Assets.Scripts.MVC;
using Assets.Scripts.TextureLists.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.TextureLists.Services;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UI.Interfaces;
using Genrpg.Shared.UnitEffects.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SearchService;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace Assets.Scripts.UI.Crawler.StatusUI
{



    public class PartyMemberStatusRow : BaseViewController<int,IView>
    {
        class Names
        {
            public const string Index = "Index";
            public const string Name = "Name";
            public const string Class = "Class";
            public const string Level = "Level";
            public const string Health = "Health";
            public const string Mana = "Mana";
            public const string Summons = "Summons";
            public const string Status = "Status";
        }

        private IStatusEffectService _statusEffectService;
        private ICrawlerService _crawlerService;

        public IButton Button;

        private AnimatedTexture _portrait;

        private GameObject _root = null;

        private PartyMember _partyMember = null;

        private Dictionary<string, IText> _textFields = new Dictionary<string, IText>();

        public override async Task Init(int partyMemberIndex, IView view, CancellationToken token)
        {
            await base.Init(partyMemberIndex, view, token);
            List<string> names = new List<string>()
            { 
                Names.Index, 
                Names.Name,
                Names.Class,
                Names.Level,
                Names.Health,
                Names.Mana, 
                Names.Summons,
                Names.Status,
            };

            Button = view.Get<IButton>("Button");
            _root = view.Get<GameObject>("Root");

            foreach (string name in names)
            {
                _textFields[name] = view.Get<IText>(name);
            }

            _portrait = view.Get<AnimatedTexture>("Portrait");

            _uiService.SetButton(Button, GetType().Name, ClickPartyMember);

            if (_model > 0)
            {
                SetText(Names.Index, _model.ToString());
            }
            else
            {
                SetText(Names.Index, "");
                SetText(Names.Name, "Name");
                if (_textFields.TryGetValue(Names.Name, out IText textField)) 
                {
                    _uiService.SetTextAlignemnt(textField, -1);
                }
                SetText(Names.Class, "Cl");
                SetText(Names.Level, "Lev");
                SetText(Names.Health, "Health");
                SetText(Names.Mana, "Mana");
                SetText(Names.Summons, "Summon");
                SetText(Names.Status, "Status");

            }

            _updateService.AddUpdate(this, OnLateUpdate, UpdateType.Late, GetToken());
            UpdateData();
        }

        private void ClickPartyMember()
        {
            _partyMember = _crawlerService.GetParty().GetMemberInSlot(_model);

            if (_partyMember == null)
            {
                return;
            }
            
            if (_crawlerService.GetState() != ECrawlerStates.ExploreWorld)
            {
                return;
            }
            _crawlerService.ChangeState(ECrawlerStates.PartyMember, _token, _partyMember, _crawlerService.GetState());

        }

        private bool _needToUpdate = false;
        public void UpdateData()
        {
            _needToUpdate = true;
        }


        private void OnLateUpdate()
        {
            if (_needToUpdate)
            {
                UpdateDataInternal();
                _needToUpdate = false;
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
                SetText(Names.Name, "");
                SetText(Names.Health, "");
                SetText(Names.Mana, "");
                SetText(Names.Class, "");
                SetText(Names.Level, "");
                SetText(Names.Summons, "");
                SetText(Names.Status, "");
                SetPortrait(null);
                return;
            }
            else
            {
                _clientEntityService.SetActive(_root, true);
                SetText(Names.Name, _partyMember.Name);
                SetText(Names.Level, _partyMember.Level.ToString());
                long currHp = _partyMember.Stats.Curr(StatTypes.Health);
                long maxHp = _partyMember.Stats.Max(StatTypes.Health);
                SetText(Names.Health, currHp + "/" + maxHp);
                long currMana = _partyMember.Stats.Curr(StatTypes.Mana);
                long maxMana = _partyMember.Stats.Max(StatTypes.Mana);

                if (maxMana < 1)
                {
                    SetText(Names.Mana, "");
                }
                else
                {
                    SetText(Names.Mana, currMana + "/" + maxMana);
                }

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

                SetText(Names.Class, classText);

                SetText(Names.Status, _statusEffectService.ShowStatusEffects(_partyMember, true));


                if (_partyMember.Summons != null && _partyMember.Summons.Count > 0)
                {
                    SetText(Names.Summons, _partyMember.Summons[0].Name);
                }
                else
                {
                    SetText(Names.Summons, "");
                }
            }
        }
        private void SetText(string fieldName, string text)
        {
            if (_textFields.TryGetValue(fieldName, out IText textField))
            {
                _uiService.SetText(textField, text);
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
