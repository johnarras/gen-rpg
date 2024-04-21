using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.UI.Services
{
    public interface IUIInitializable : IInitializable
    {
        void SetText(GText gtext, string txt);
        void SetInputText(GInputField input, object obj);
        int GetIntInput(GInputField field);
        long GetSelectedIdFromName(Type iidNameType, GDropdown dropdown);
        void SetImageTexture(GRawImage image, UnityEngine.Texture tex);
        GEntity GetSelected();
        void SetColor(GText text, UnityEngine.Color color);
        void SetButton(GButton button, string screenName, UnityAction action, Dictionary<string, string> extraData = null);
        void AddEventListener(UnityGameState gs, GEntity go, EventTriggerType type, UnityAction<BaseEventData> callback);
    }
}
