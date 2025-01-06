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

        public GButton ClassButton;
        public GButton RaceButton;
        public GButton SpellButton;
        public GButton MonsterButton;

        public GameObject ListAnchor;
        public GameObject InfoAnchor;


        public GText ListText;
        public GText InfoText;


        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            _uiService.SetButton(ClassButton, GetName(), () => { ShowRoleList(RoleCategories.Class); });
            _uiService.SetButton(RaceButton, GetName(), () => { ShowRoleList(RoleCategories.Origin); });
            _uiService.SetButton(SpellButton, GetName(), () => { ShowInfoList(EntityTypes.CrawlerSpell); });
            _uiService.SetButton(MonsterButton, GetName(), () => { ShowInfoList(EntityTypes.Unit); });

            _clientEntityService.SetActive(ListText, false);
            _clientEntityService.SetActive(InfoText, false);

            await Task.CompletedTask;
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
            ClearInfo();
        }

        private void ClearList()
        {
            _clientEntityService.DestroyAllChildren(ListAnchor);
        }

        private void ClearInfo()
        {         
            _clientEntityService.DestroyAllChildren(InfoAnchor);
        }

        private void ShowInfo(long entityTypeId, long entityId)
        {
            ClearInfo();
            List<string> lines = _infoService.GetInfoLines(entityTypeId, entityId);


            foreach (string line in lines)
            {
                GText text = _clientEntityService.FullInstantiate<GText>(InfoText);

                _clientEntityService.AddToParent(text, InfoAnchor);

                _uiService.SetText(text, line);

            }
        }

        private void ShowChildList<T>(List<T> list, long entityTypeId) where T : IIdName
        {

            ClearAllChildren();


            foreach (IIdName idname in list)
            {
                GText text = _clientEntityService.FullInstantiate<GText>(ListText);

                _clientEntityService.AddToParent(text, ListAnchor);

                _uiService.SetText(text, idname.Name);

                _uiService.AddPointerHandlers(text, () => { ShowInfo(entityTypeId, idname.IdKey); }, ClearInfo);


            }
        }
    }
}
