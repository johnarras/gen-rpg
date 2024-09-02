using Assets.Scripts.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Screens.Constants;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class WorldPanelButtons : BaseBehaviour
    {
        public GButton MapButton;

        private IUIService _uiService;
        public override void Init()
        {
            _uiService.SetButton(MapButton, name, ClickMapScreen);
        }

        private void ClickMapScreen()
        {
            _screenService.Open(ScreenId.CrawlerMap);
        }
    }
}
