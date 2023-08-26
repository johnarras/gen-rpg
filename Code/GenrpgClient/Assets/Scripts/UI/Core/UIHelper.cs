using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.Core.Entities;

using Services;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UI.Screens.Constants;

public class UIHelper
{


    private static UnityGameState _gs = null;
    public static void SetGameState(UnityGameState gs)
    {
        _gs = gs;
    }

    public static void SetText (Text text, string txt)
    {
        if (text == null)
        {
            return;
        }
        text.text = txt;
    }
    public static void SetButton(Button button, string screenName, UnityAction action, Dictionary<string,string> extraData = null)
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

    public static void SetColor (Text text, Color color)
    {
        if (text == null)
        {
            return;
        }

        text.color = color;
    }

    public static string GetInputText (InputField field)
    {
        if (field == null)
        {
            return "";
        }

        return field.text;
    }

    public static GameObject GetSelected()
    {
        if (EventSystem.current == null)
        {
            return null;
        }
        return EventSystem.current.currentSelectedGameObject;
    }

    public static void SetSprite(Image image, Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
    }

    public static void SetImageTexture(RawImage image, Texture tex)
    {
        if (image == null)
        {
            return;
        }

        image.texture = tex;
    }



}