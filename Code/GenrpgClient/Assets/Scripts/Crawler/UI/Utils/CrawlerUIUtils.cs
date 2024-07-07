using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.TextCore.Text;

namespace Assets.Scripts.Crawler.UI.Utils
{
    public static class CrawlerUIUtils
    {
        public const string ColorWhite = "#FFFFFF";
        public const string ColorYellow = "#FFD800";

        public static string HighlightText(string text, string color = ColorYellow)
        {
            return $"<color={color}>{text}</color>";
        }

        public static string HighlightText(char c, string color = ColorYellow) 
        {
            return $"<color={color}>{c}</color>";
        }
    }
}
