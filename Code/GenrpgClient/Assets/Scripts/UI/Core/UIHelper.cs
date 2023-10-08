using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine; // Fix

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



}