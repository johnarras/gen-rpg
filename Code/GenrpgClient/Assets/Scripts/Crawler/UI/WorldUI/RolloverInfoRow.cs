using Assets.Scripts.ClientEvents;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class RolloverInfoRow : BaseBehaviour
    {
        public GText MainText;
        protected ITextService _textService;
        protected IInfoService _infoService;

        public override void Init()
        {
            _uiService.AddPointerHandlers(MainText, OnPointerEnter, OnPointerExit);
        }


        public virtual void OnPointerExit()
        {
            _dispatcher.Dispatch(new HideInfoPanelEvent());
        }

        public virtual void OnPointerEnter()
        {

            List<string> lines = _infoService.GetInfoLines(_textService.GetLinkUnderMouse(MainText));

            if (lines.Count > 0)
            {
                _dispatcher.Dispatch(new ShowInfoPanelEvent() { Lines = lines });
            }
        }
    }
}
