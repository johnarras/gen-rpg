using GameEditor;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Utils.Data;
using System;
using System.Windows.Forms;

namespace Genrpg.Editor
{
    public class UserControlFactory
    {
        public UserControl Create(EditorGameState gs, DataWindow win, object obj, object parent, object grandparent)
        {
            UserControl uc = GetOverrideControl(gs, win, obj, parent, grandparent);
            if (uc != null)
            {
                return uc;
            }

            uc = new DataView(gs, win, obj, parent, grandparent);
            return uc;
        }
        // Use this to create custom controls for certain types or whatever you want.
        protected UserControl GetOverrideControl(EditorGameState gs, DataWindow win, object obj, object parent, object grandparent)
        {

            MyColorF mc = obj as MyColorF;
            if (mc != null)
            {
                return new MyColorFDataView(gs, win, obj, parent, grandparent);
            }


            return null;

        }


    }
}
