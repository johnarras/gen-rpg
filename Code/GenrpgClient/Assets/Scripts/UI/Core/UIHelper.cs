using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine; // Fix
using Genrpg.Shared.Interfaces;
using System.Linq;

public class UIHelper
{
    private static UnityGameState _gs = null;
    public static void SetGameState(UnityGameState gs)
    {
        _gs = gs;
    }

    public static void SetText (GText text, string txt)
    {
        if (text == null)
        {
            return;
        }
        text.text = txt;
    }

    public static void SetInputText(GInputField input, object obj)
    {
        if (input != null && obj != null)
        {
            input.text = obj.ToString();
        }
    }

    public static void SetButton(GButton button, string screenName, UnityAction action, Dictionary<string,string> extraData = null)
    {
        if (button == null || action == null)
        {
            return;
        }
        button.onClick.AddListener(
           () => {
               Analytics.Send(AnalyticsEvents.ClickButton, button.name, screenName, extraData);
               action();
           });
            
    }

    public static void SetColor (GText text, UnityEngine.Color color)
    {
        if (text == null)
        {
            return;
        }

        text.color = color;
    }

    public static GEntity GetSelected()
    {
        if (EventSystem.current == null)
        {
            return null;
        }
        return EventSystem.current.currentSelectedGameObject;
    }

    public static void SetSprite(GImage image, Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
    }

    public static void SetImageTexture(GRawImage image, Texture tex)
    {
        if (image == null)
        {
            return;
        }

        image.texture = tex;
    }

    public static long GetSelectedIdFromName(Type iidNameType,  GDropdown dropdown)
    {
        List<IIdName> items = _gs.data.GetList(iidNameType.Name);

        string selectedText = dropdown.captionText.text;

        IIdName selectedItem = items.FirstOrDefault(x => x.Name == dropdown.captionText.text);

        return selectedItem?.IdKey ?? 0;

    }

    public static int GetIntInput(GInputField field)
    {
        if (field == null)
        {
            return 0;
        }

        if (Int32.TryParse(field.text, out int value))
        {
            return value;
        }

        return 0;
    }

    public static float GetFloatInput(GInputField field)
    {
        if (field == null)
        {
            return 0;
        }

        if (float.TryParse(field.text, out float value))
        {
            return value;
        }
        return 0;
    }
}