using Genrpg.Editor.UI.Constants;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Genrpg.Editor.UI.Formatters
{
    public class CheckBoxFormatter
    {
        public const int DefaultBG = FormFormatter.DefaultBG + 20;
        public const int DefaultFG = 255 - DefaultBG;

        public Color BackColor { get; set; } = Color.White;
        public Color ForeColor { get; set; } = Color.Black;


        public CheckBoxFormatter()
        {
            BackColor = UIFormatter.CreateGray(DefaultBG);
            ForeColor = UIFormatter.CreateGray(DefaultFG);
        }

        public void Format(CheckBox CheckBox)
        {
            CheckBox.BackColor = BackColor;
            CheckBox.ForeColor = ForeColor;
            CheckBox.FlatStyle = FlatStyle.Flat;

            Font font = CheckBox.Font;

            Font newFont = new Font(familyName: FormatterConstants.DefaultFontFamily, style: FontStyle.Bold, emSize: (font != null ? font.Size : FormatterConstants.SmallLabelFontSize));

        }
    }
}
