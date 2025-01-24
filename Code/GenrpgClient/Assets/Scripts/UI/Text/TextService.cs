using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.UI.Interfaces;
using Genrpg.Shared.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace Assets.Scripts.UI.Services
{
    public class TextService : ITextService
    {
        private IInputService _inputService;

        public string GetLinkUnderMouse(IText text)
        {
            if (text is GText gtext)
            {
                int linkIndex = TMP_TextUtilities.FindIntersectingLink(gtext, _inputService.MousePosition(), null);

                if (linkIndex < 0)
                {
                    linkIndex = TMP_TextUtilities.FindNearestLink(gtext, _inputService.MousePosition(), null);
                }

                if (linkIndex >= 0 && gtext.textInfo.linkInfo.Length > linkIndex)
                {
                    TMP_LinkInfo linkInfo = gtext.textInfo.linkInfo[linkIndex]; 
                    string linkID = linkInfo.GetLinkID();
                    if (!string.IsNullOrEmpty(linkID))
                    {
                        return linkID;
                    }
                }
            }
            return null;
        }

        public string HighlightText(string text, string color = TextColors.ColorGold)
        {
            return $"<color={color}>{text}</color>";
        }

        public string HighlightText(char c, string color = TextColors.ColorGold)
        {
            return $"<color={color}>{c}</color>";
        }

    }
}
