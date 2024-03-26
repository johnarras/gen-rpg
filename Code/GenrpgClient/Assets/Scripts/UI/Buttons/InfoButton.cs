using Assets.Scripts.UI.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Ftue.Constants;
using Genrpg.Shared.Ftue.PlayerData;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Ftue.Settings.Steps;
using Genrpg.Shared.Logging.Interfaces;

public class InfoButton : BaseBehaviour
{
    private IFtueService _ftueService = null;
    protected IRepositoryService _repoService;
    public GButton Button;

    private string _screenName = null;
    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        BaseScreen screen = GEntityUtils.FindInParents<BaseScreen>(gameObject);

        if (screen != null)
        {
            _screenName = screen.GetName();
            _uiService.SetButton(Button, screen.GetName(), ClickInfoButton);
        }
    }

    private void ClickInfoButton()
    {
       // if (_ftueService.GetCurrentStep(_gs,_gs.ch) == null)
        {
            FtueStep step = _gs.data.Get<FtueStepSettings>(_gs.ch).FindFtueStep(FtueTriggers.InfoButton, _screenName);

            if (step != null)
            {

                _ftueService.StartStep(_gs, _gs.ch, step.IdKey);

                _logService.Info("Show Info for " + _screenName);
            }
        }
    }
}
