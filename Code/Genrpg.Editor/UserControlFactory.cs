using GameEditor;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.UI;
using Genrpg.Shared.Utils.Data;
using System;
using System.Windows.Forms;

namespace Genrpg.Editor
{
    public class UserControlFactory
    {
        private UIFormatter _formatter = null;
        public UserControl Create(EditorGameState gs, UIFormatter formatter, DataWindow win, object obj, object parent, object grandparent, DataView parentView)
        {
            _formatter = formatter;
            UserControl uc = GetOverrideControl(gs, formatter, win, obj, parent, grandparent, parentView);
            if (uc != null)
            {
                return uc;
            }

            uc = new DataView(gs, formatter, win, obj, parent, grandparent, parentView);
            return uc;
        }
        // Use this to create custom controls for certain types or whatever you want.
        protected UserControl GetOverrideControl(EditorGameState gs, UIFormatter formatter, DataWindow win, object obj, object parent, object grandparent, DataView parentView)
        {

            MyColorF mc = obj as MyColorF;
            if (mc != null)
            {
                return new MyColorFDataView(gs, formatter, win, obj, parent, grandparent, parentView);
            }


            return null;

        }


    }
}
