using Genrpg.Editor.UI.Constants;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Genrpg.Editor.UI.FormatterHelpers
{
    public class LabelFormatter
    {
        public const int DefaultGray = 240;

        public Color TextColor { get; set; } = Color.White;
        public string FamilyName { get; set; } = FormatterConstants.DefaultFontFamily;
        public FontStyle FontStyle { get; set; } = FormatterConstants.DefaultFontStyle;


        public LabelFormatter()
        {
            TextColor = UIFormatter.CreateGray(DefaultGray);
        }

        public void Format(Label label, float fontSize)
        {
            Font font = label.Font;

            label.ForeColor = TextColor;
            Font newFont = new Font(familyName:FamilyName, emSize:fontSize > 0 ? fontSize : font.Size, style:FontStyle);

            label.Font = newFont;

        }

    }
}
