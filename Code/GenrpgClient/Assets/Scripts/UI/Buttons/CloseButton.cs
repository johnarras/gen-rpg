using Assets.Scripts.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CloseButton : BaseBehaviour
{
    public GButton Button;

    public override void Init()
    {
        base.Init();
        BaseScreen screen = _clientEntityService.FindInParents<BaseScreen>(gameObject);

        if (screen != null)
        {
            _uiService.SetButton(Button, screen.GetName(), screen.StartClose);
        }
    }
}
