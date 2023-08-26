using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Genrpg.Editor
{
    public class UIHelper
    {
        public static Form ShowBlockingDialog(string text, Form owner = null, int width = 0, int height = 0)
        {
            Form f = new Form();
            f.Owner = owner;

            if (width == 0)
            {
                width = 150;
            }

            if (height == 0)
            {
                height = 100;
            }

            Label lab = new Label();

            lab.Size = new Size(width - 20, height - 40);
            lab.TextAlign = ContentAlignment.MiddleCenter;
            lab.Text = text;
            f.Controls.Add(lab);
            f.Size = new Size(width, height);
            f.Show();
            f.Refresh();
            return f;
        }


    }
}
