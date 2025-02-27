
using System;

namespace Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents
{
    public class AddActionPanelText
    {
        public string Text { get; set; }
        public Action OnClickAction { get; set; }


        public AddActionPanelText(string text, Action onClickAction = null)
        {
            Text = text;
            OnClickAction = onClickAction;
        }
    }
}
