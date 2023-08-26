using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Screens.Constants;

namespace UI.Screens.Utils
{
    public class ScreenUtils
    {
        public static string GetPrefabName(ScreenId id)
        {
            return (id.ToString() + "Screen");
        }
    }
}
