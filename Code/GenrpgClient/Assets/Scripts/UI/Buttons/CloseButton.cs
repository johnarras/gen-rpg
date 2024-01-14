using Assets.Scripts.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CloseButton : BaseBehaviour
{
    private IUIService _uiService = null;
    public GButton Button;

    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        BaseScreen screen = GEntityUtils.FindInParents<BaseScreen>(gameObject);

        if (screen != null)
        {
            _uiService.SetButton(Button, screen.GetName(), screen.StartClose);
        }
    }
}
