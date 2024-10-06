using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UI.Entities
{
    public class ActiveScreen
    {
        public IScreen Screen;
        public ScreenLayers LayerId;
        public ScreenId ScreenId;
        public object Data;
        public object LayerObject;
    }
}
