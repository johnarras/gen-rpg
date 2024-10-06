using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Client.GameEvents
{

    public enum EFloatingTextArt
    {
        Message = 0,
        Error = 1,
    }

    public class ShowFloatingText
    {

        public ShowFloatingText(string text, EFloatingTextArt art = EFloatingTextArt.Message)
        {
            Text = text;
            Art = art;
        }

        public string Text;
        public EFloatingTextArt Art;
    }

}
