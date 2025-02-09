using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.MainMenu
{
    public class LoadSaveButton : BaseBehaviour
    {

        public GButton Button;
        public GText Slot;
        public GText Name;
        public GText UpdateTime;

        public GImage SelectedImage;

        private int _slot = 0;
        private string _saveName;
        private DateTime _updateTime = DateTime.MinValue;

        private LoadSaveScreen _screen;

        public void Init(LoadSaveScreen screen, int slot, INamedUpdateData data)
        {
            _screen = screen;
            _slot = slot;


            if (data != null)
            {
                _saveName = data.Name;
                _updateTime = data.UpdateTime;
            }

            _uiService.SetText(Slot, _slot.ToString() + ".");

            _uiService.SetText(Name, !string.IsNullOrEmpty(_saveName) ? _saveName : " -- ");

            _uiService.SetText(UpdateTime, _updateTime > DateTime.MinValue ? _updateTime.ToString() : " -- ");

            _uiService.SetButton(Button, _screen.GetName(), OnClickButton);

            SetHighlight(false);
        }

        private void OnClickButton()
        {
            _screen.SetSlot(_slot);
        }

        public void SetHighlight(bool visible)
        {

            _clientEntityService.SetActive(SelectedImage, visible);
        }
    }
}
