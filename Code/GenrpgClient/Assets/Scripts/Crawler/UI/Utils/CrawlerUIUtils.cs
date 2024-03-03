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

        const string _highlightColor = "#FFD800";
        public static string HighlightText(string text)
        {
            return $"<color={_highlightColor}>{text}</color>";
        }

        public static string HighlightText(char c)
        {

            return $"<color={_highlightColor}>{c}</color>";
        }
    }
}
