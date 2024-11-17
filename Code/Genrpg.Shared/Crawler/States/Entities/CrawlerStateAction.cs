using Genrpg.Shared.Crawler.States.Constants;
using System;

namespace Genrpg.Shared.Crawler.States.Entities
{
    public class CrawlerStateAction
    {
        public CrawlerStateAction(string text,
            char key = '\0',
            ECrawlerStates nextState = ECrawlerStates.None,
            Action onClickAction = null,
            object extraData = null,
            string spriteName = null,
            Action pointerEnterAction = null,
            Action pointerExitAction = null,
            bool rowFiller = false,
            bool forceButton = false,
            bool forceText = false)
        {
            Text = text;
            Key = key;
            NextState = nextState;
            OnClickAction = onClickAction;
            SpriteName = spriteName;
            ExtraData = extraData;
            OnPointerEnter = pointerEnterAction;
            OnPointerExit = pointerExitAction;
            RowFiller = rowFiller;
            ForceButton = forceButton;
            ForceText = forceText;
        }

        public string Text { get; private set; }
        public char Key { get; private set; }
        public ECrawlerStates NextState { get; private set; }
        public Action OnClickAction { get; private set; }
        public string SpriteName { get; private set; }
        public object ExtraData { get; private set; }
        public Action OnPointerEnter { get; private set; }
        public Action OnPointerExit { get; private set; }
        public bool RowFiller { get; private set; }
        public bool ForceButton { get; private set; }
        public bool ForceText { get; private set; }

    }
}
