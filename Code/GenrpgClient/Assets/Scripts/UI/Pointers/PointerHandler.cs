using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Pointers
{
    public class PointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        private Action _enterHandler;
        public void OnPointerEnter(PointerEventData eventData)
        {
            _enterHandler?.Invoke();
        }

        private Action _exitHandler;
        public void OnPointerExit(PointerEventData eventData)
        {
            _exitHandler?.Invoke();
        }

        public void SetEnterExitHandlers(Action enterHandler,  Action exitHandler)
        {
            _enterHandler = enterHandler;
            _exitHandler = exitHandler;
        }
    }
}
