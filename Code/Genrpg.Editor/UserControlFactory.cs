using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.UI;
using Genrpg.Shared.Utils.Data;
using Microsoft.UI.Xaml.Controls;

namespace Genrpg.Editor
{
    public class UserControlFactory
    {
        public UserControl Create(EditorGameState gs,DataWindow win, object obj, object parent, object grandparent, DataView parentView)
        {
            UserControl uc = GetOverrideControl(gs, win, obj, parent, grandparent, parentView);
            if (uc != null)
            {
                return uc;
            }

            uc = new DataView(gs, win, obj, parent, grandparent, parentView);
            return uc;
        }
        // Use this to create custom controls for certain types or whatever you want.
        protected UserControl GetOverrideControl(EditorGameState gs,DataWindow win, object obj, object parent, object grandparent, DataView parentView)
        {

            MyColorF mc = obj as MyColorF;
            if (mc != null)
            {
                //return new ColorDataViewOld(gs, win, obj, parent, grandparent, parentView);
            }


            return null;

        }


    }
}
