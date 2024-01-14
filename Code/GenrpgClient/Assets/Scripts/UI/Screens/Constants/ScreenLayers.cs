using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;

namespace UI.Screens.Constants
{
    // These do not have to be in order. The Layers list inside of the ScreenManager (ScreenService)
    // creates the order.
    public enum ScreenLayers
    {
        Screens = 0,
        HUD =1,
        Loading=2,
        Popups=3,
        Overlays=4,
        FloatingText=5,
        DragItems=6,
        Blocker=7,
    }
}
