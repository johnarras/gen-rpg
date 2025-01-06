using Assets.Scripts.UI.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Core
{
    public class NamedSlider : BaseBehaviour
    {
        public GText Name;
        public GSlider Slider;

        private Action<float> _valueChangedEvent;
        public void InitSlider(float minValue, float maxValue, float currValue, Action<float> valueChangedEvent)
        {
            Slider.Init(minValue, maxValue, currValue, valueChangedEvent);
        }
    }
}
