using Assets.Scripts.Info.UI;
using Assets.Scripts.UI.Crawler.ActionUI;
using Assets.Scripts.UI.Crawler.WorldUI;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.Screens.Info
{
    public class CrawlerInfoScreen : BaseScreen
    {

        protected IInfoService _infoService;
        protected IEntityService _entityService;
        private IInputService _inputService;

        public GButton ClassButton;
        public GButton RaceButton;
        public GButton SpellButton;
        public GButton MonsterButton;

        public GameObject ListAnchor;

        public InfoPanel InfoPanel;


        public GText ListText;


        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            _uiService.SetButton(ClassButton, GetName(), () => { ShowRoleList(RoleCategories.Class); });
            _uiService.SetButton(RaceButton, GetName(), () => { ShowRoleList(RoleCategories.Origin); });
            _uiService.SetButton(SpellButton, GetName(), () => { ShowInfoList(EntityTypes.CrawlerSpell); });
            _uiService.SetButton(MonsterButton, GetName(), () => { ShowInfoList(EntityTypes.Unit); });

            _clientEntityService.SetActive(ListText, false);

            await Task.CompletedTask;
        }

        protected override void ScreenUpdate()
        {
            base.ScreenUpdate();
            if (_inputService.GetKeyDown(CharCodes.Escape))
            {
                InfoPanel.PopInfoStack();
            }
        }

        private void ShowRoleList(long roleCategoryId)
        {
            List<Role> roles = _gameData.Get<RoleSettings>(_gs.ch).GetData().Where(x=>x.RoleCategoryId == roleCategoryId).ToList();

            ShowChildList(roles, EntityTypes.Role);
        }

        private void ShowInfoList(long entityTypeId)
        {
            List<IIdName> children = _entityService.GetChildList(_gs.ch, entityTypeId);


            ShowChildList(children, entityTypeId);

        }


        private void ClearAllChildren()
        {
            ClearList();
            InfoPanel.ClearInfo();
        }

        private void ClearList()
        {
            _clientEntityService.DestroyAllChildren(ListAnchor);
        }

   

        public void ShowChildList<T>(List<T> list, long entityTypeId) where T : IIdName
        {

            InfoPanel.ClearStack();

            ClearAllChildren();


            foreach (IIdName idname in list)
            {
                GText text = _clientEntityService.FullInstantiate<GText>(ListText);

                _clientEntityService.AddToParent(text, ListAnchor);

                _uiService.SetText(text, idname.Name);

                _uiService.AddPointerHandlers(text, () => { InfoPanel.ShowInfo(entityTypeId, idname.IdKey); }, () => { });


            }
        }
    }
}
