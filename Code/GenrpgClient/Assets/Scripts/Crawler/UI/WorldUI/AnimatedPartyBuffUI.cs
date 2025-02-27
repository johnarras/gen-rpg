using Assets.Scripts.Assets.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class AnimatedPartyBuffUI : PartyBuffUI
    {
        public string ImageName;
        public AnimatedSprite Sprite;

        public override void Init()
        {
            base.Init();

            Sprite.SetImage(ImageName);
        }
    }
}
