using Assets.Scripts.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CloseButton : BaseBehaviour
{
    public GButton Button;

    public override void Initialize(IUnityGameState gs)
    {
        base.Initialize(gs);
        BaseScreen screen = GEntityUtils.FindInParents<BaseScreen>(gameObject);

        if (screen != null)
        {
            _uIInitializable.SetButton(Button, screen.GetName(), screen.StartClose);
        }
    }
}
