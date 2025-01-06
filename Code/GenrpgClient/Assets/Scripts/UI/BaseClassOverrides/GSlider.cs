using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Abstractions
{
    public class GSlider : Slider
    {
        private Action<float> _valueChangedEvent;
        public void Init(float minValueIn, float maxValueIn, float currValue, Action<float> valueChangedEvent)
        {
            minValue = minValueIn;
            maxValue = maxValueIn;
            value = currValue;

            onValueChanged.RemoveAllListeners();
            onValueChanged.AddListener(OnSliderMoved);

            _valueChangedEvent = valueChangedEvent;
        }


        private void OnSliderMoved(float val)
        {
            _valueChangedEvent?.Invoke(val);
        }
    }
}
