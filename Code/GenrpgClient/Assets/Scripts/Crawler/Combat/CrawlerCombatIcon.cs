using Assets.Scripts.Assets.Textures;
using Assets.Scripts.ClientEvents;
using Assets.Scripts.UI.CombatTexts;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.TextureLists.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Units.Settings;
using System;
using System.Linq;

namespace Assets.Scripts.Crawler.Combat
{
    public class CrawlerCombatIcon : BaseBehaviour
    {

        private ITextureListCache _cache;

        public AnimatedSprite Icon;
        public GText Info;
        public CombatGroup Group;
        public FastCombatTextUI TextUI;
        public GButton Button;


        private Action _clickAction;

        public override void Init()
        {
            base.Init();
            _dispatcher.AddListener<SetCombatGroupAction>(OnSetCombatGroupAction, GetToken());
            _dispatcher.AddListener<ClearCombatGroupActions>(OnClearCombatGroupActions, GetToken());
            _uiService.SetButton(Button, GetType().Name, OnClickButton);

        }

        public void UpdateData()
        {
            if (Group == null)
            {
                return;
            }

            if (TextUI != null)
            {
                TextUI.SetGroupId(Group.Id);
            }

            UnitType unitType = _gameData.Get<UnitSettings>(_gs.ch).Get(Group.UnitTypeId);

            if (unitType ==null)
            {
                return;
            }
            int okUnitCount = Group.Units.Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead)).Count();

            _uiService.SetText(Info, okUnitCount + " " + (okUnitCount == 1 ? unitType.Name : unitType.PluralName) + " - " + Group.Range + "'");

            Icon.SetImage(unitType.Icon);

        }

        private void OnClickButton()
        {
            _clickAction?.Invoke();
        }

        private void OnClearCombatGroupActions(ClearCombatGroupActions clear)
        {
            _clickAction = null;
        }

        private void OnSetCombatGroupAction(SetCombatGroupAction setAction)
        {

            if (Group != null && Group == setAction.Group)
            {
                _clickAction = setAction.Action;
            }
        }
    }
}
