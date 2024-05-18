using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
