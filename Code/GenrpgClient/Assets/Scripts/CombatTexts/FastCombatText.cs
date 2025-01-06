using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.CombatTexts
{
    public class FastCombatText : BaseBehaviour
    {

        public float ElapsedSeconds { get; set; }
        public float Angle { get; set; }
        public float Speed { get; set; }
        public float FrameDx { get; set; } = 0;
        public float FrameDy { get; set; } = 1;
        public GText Text;
        public void ShowText(string text)
        {
            _uiService.SetText(Text, text);

        }
    }
}
