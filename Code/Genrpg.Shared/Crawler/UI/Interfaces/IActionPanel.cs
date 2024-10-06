using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.UI.Interfaces
{
    public interface IActionPanel
    {
        void Clear();

        void AddText(string text, Action onClickHandler = null);
    }
}
