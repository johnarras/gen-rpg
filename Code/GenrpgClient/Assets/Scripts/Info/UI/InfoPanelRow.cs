using Assets.Scripts.Crawler.UI.Screens.Info;
using Genrpg.Shared.Crawler.Info.Constants;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Info.UI
{
    public class InfoPanelRow : BaseBehaviour
    {

        private IInfoService _infoService;
        private ITextService _textService;
        public GText Text;
        public GButton Button;

        private string _text;

        private InfoPanel _panel;
        public void InitData(InfoPanel panel, string text)
        {
            _panel = panel;
            _text = text;
            _uiService.SetText(Text, _text);

            if (string.IsNullOrEmpty(_text))
            {
                return;
            }

            if (_text.IndexOf(InfoConstants.LinkPrefix) > 0 && _text.IndexOf(InfoConstants.LinkMiddle) > _text.IndexOf(InfoConstants.LinkPrefix))
            {
                _uiService.SetButton(Button, GetType().Name, OnClickText);
            }
        }

        protected virtual void OnClickText()
        {
            if (string.IsNullOrEmpty(_text))
            {
                return;
            }

            _panel.ShowLines(_infoService.GetInfoLines(_textService.GetLinkUnderMouse(Text)));

        }
    }
}
