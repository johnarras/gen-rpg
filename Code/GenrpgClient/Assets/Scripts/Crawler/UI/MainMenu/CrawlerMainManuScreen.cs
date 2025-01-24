using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.MainMenu
{
    public class CrawlerMainManuScreen : MainMenuScreen
    {

        protected IInputService _inputService;

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await base.OnStartOpen(data, token);
        }

        protected override void ScreenUpdate()
        {
            base.ScreenUpdate();

            if (_inputService.GetKeyDown(CharCodes.Escape))
            {
                _screenService.Close(ScreenId.CrawlerMainMenu);
            }
        }
    }
}
