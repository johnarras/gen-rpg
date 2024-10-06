using Genrpg.Shared.Client.Tokens;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.MVC.Interfaces;

namespace Genrpg.Shared.UI.Services
{
    public interface IUIService : IInitializable, IGameTokenService
    {
        void SetText(IText gtext, string txt);
        void SetInputText(IInputField input, object obj);
        int GetIntInput(IInputField field);
        long GetSelectedIdFromName(Type iidNameType, IDropdown dropdown);
        void SetImageTexture(IRawImage image, object tex);
        object GetImageTexture(IRawImage image);
        int GetImageHeight(IRawImage image);
        int GetImageWidth(IRawImage image);
        void SetUVRect(IRawImage image, float xpos, float ypos, float xsize, float ysize);
        object GetSelected();
        void SetColor(IText text, object color);
        void SetButton(IButton button, string screenName, Action action, Dictionary<string, string> extraData = null);
        void SetButton(IButton button, string screenName, Func<CancellationToken, Task> awaitableAction, Dictionary<string, string> extraData = null);
        void SetAlpha(IText text, float alpha);
        void SetAutoSizing(IText text, bool autoSizing);
        void ResizeGridLayout(IGridLayoutGroup group, float xscale, float yscale);
        void AddPointerHandlers(IView view, Action enterHandler, Action exitHandler);
        void ScrollToBottom(object scrollRectObj);
        void ScrollToTop(object scrollRectObj);
        void SetTextAlignemnt(IText text, int offset); // -1,0,1= left, center, right
    }
}
