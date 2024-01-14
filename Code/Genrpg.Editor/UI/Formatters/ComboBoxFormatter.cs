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
    public class ComboBoxFormatter
    {
        public const int DefaultBG = FormFormatter.DefaultBG + 20;
        public const int DefaultFG = 255 - DefaultBG;

        public Color BackColor { get; set;  } = Color.White;
        public Color ForeColor { get; set; } = Color.Black;


        public ComboBoxFormatter()
        {
            BackColor = UIFormatter.CreateGray(DefaultBG);
            ForeColor = UIFormatter.CreateGray(DefaultFG);
        }

        public void Format(ComboBox comboBox)
        {
            comboBox.BackColor = BackColor;
            comboBox.ForeColor = ForeColor;
            comboBox.FlatStyle = FlatStyle.Flat;

            Font font = comboBox.Font;

            Font newFont = new Font(familyName: FormatterConstants.DefaultFontFamily, style: FontStyle.Bold, emSize: (font != null ? font.Size : FormatterConstants.SmallLabelFontSize));

            comboBox.Font = font;
        }
    }
}
