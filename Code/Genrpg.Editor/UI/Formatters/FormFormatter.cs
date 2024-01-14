using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Genrpg.Editor.UI.Formatters
{
    public class FormFormatter
    {
        public const int DefaultBG = 37;
        public const int DefaultFG = 0;

        public Color BackColor { get; set; } = Color.DarkGray;
        public Color ForeColor { get; set; } = Color.Black;

        public FormFormatter()
        {
            BackColor = UIFormatter.CreateGray(DefaultBG);
            ForeColor = UIFormatter.CreateGray(DefaultFG);
        }

        public void Format(Form form)
        {
            form.ForeColor = ForeColor;
            form.BackColor = BackColor;
        }
    }
}
