﻿using Genrpg.Editor.UI.Constants;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Genrpg.Editor.UI.FormatterHelpers
{
    public class ButtonFormatter
    {
        public const int DefaultBG = FormFormatter.DefaultBG+40;
        public const int DefaultFG = 235; // Font Color

        public Color BackColor { get; set; } = Color.Black;
        public Color ForeColor { get; set; } = Color.White;

        public Color BorderColor { get; set; } = Color.Black;
        public int BorderSize { get; set; } = 1;

        public string FamilyName { get; set; } = FormatterConstants.DefaultFontFamily;
        public FontStyle FontStyle { get; set; } = FormatterConstants.DefaultFontStyle;

        public ButtonFormatter()
        {
            BackColor = UIFormatter.CreateGray(DefaultBG);
            ForeColor = UIFormatter.CreateGray(DefaultFG);
            BorderColor = UIFormatter.CreateGray(DefaultBG + 20);
            BorderSize = 1;
        }

        public void Format(Button button)
        {
            button.BackColor = BackColor;
            button.ForeColor = ForeColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = BorderColor;
            button.FlatAppearance.BorderSize = BorderSize;

            Font font = button.Font;

            Font newFont = new Font(familyName:FamilyName, emSize:font.Size, style:FontStyle);

            button.Font = newFont;
        }
    }
}
