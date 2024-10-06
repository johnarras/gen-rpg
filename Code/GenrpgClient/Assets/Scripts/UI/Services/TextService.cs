using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Services
{
    public class TextService : ITextService
    {
        public string HighlightText(string text, string color = TextColors.ColorYellow)
        {
            return $"<color={color}>{text}</color>";
        }

        public string HighlightText(char c, string color = TextColors.ColorYellow)
        {
            return $"<color={color}>{c}</color>";
        }

    }
}
