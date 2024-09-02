using Assets.Scripts.UI.Crawler.States;
using System;
using UnityEngine;

namespace Assets.Scripts.Crawler.CrawlerStates
{
    public class CrawlerStateAction
    {
        public CrawlerStateAction(string text,
            KeyCode key = KeyCode.None,
            ECrawlerStates nextState = ECrawlerStates.None,
            Action onClickAction = null,
            object extraData = null,
            string spriteName = null,
            Action pointerEnterAction = null,
            Action pointerExitAction = null)
        {
            Text = text;
            Key = key;
            NextState = nextState;
            OnClickAction = onClickAction;
            SpriteName = spriteName;
            ExtraData = extraData;
            OnPointerEnter = pointerEnterAction;
            OnPointerExit = pointerExitAction;
        }

        public string Text { get; private set; }
        public KeyCode Key { get; private set; }
        public ECrawlerStates NextState { get; private set; }
        public Action OnClickAction { get; private set; }
        public string SpriteName { get; private set; }
        public object ExtraData { get; private set; }
        public Action OnPointerEnter { get; private set; }
        public Action OnPointerExit { get; private set; }

    }
}
