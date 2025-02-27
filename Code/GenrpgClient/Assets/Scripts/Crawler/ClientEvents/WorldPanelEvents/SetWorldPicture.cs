using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.ClientEvents.WorldPanelEvents
{
    public class SetWorldPicture
    {
        public string SpriteName { get; set; }
        public bool UseBGOnly { get; set; }

        public SetWorldPicture(string spriteName, bool useBGOnly)
        {
            SpriteName = spriteName;
            UseBGOnly = useBGOnly;
        }
    }
}
