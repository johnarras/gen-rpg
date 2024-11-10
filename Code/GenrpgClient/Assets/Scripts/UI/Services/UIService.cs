using Assets.Scripts.Interfaces;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Ftue.Messages;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Ftue.Settings.Steps;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Scripts.Assets.Audio.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Genrpg.Shared.UI.Interfaces;
using Genrpg.Shared.UI.Services;
using Assets.Scripts.UI.Abstractions;
using Assets.Scripts.UI.Pointers;
using UnityEngine.UI;
using Genrpg.Shared.MVC.Interfaces;
using Assets.Scripts.Awaitables;

namespace Assets.Scripts.UI.Services
{
    public class UIService : IUIService
    {
        protected IFtueService _ftueService;
        protected IAudioService _audioService;
        protected IRealtimeNetworkService _realtimeNetworkService;
        protected IAnalyticsService _analyticsService;
        protected IGameData _gameData;
        protected IClientRandom _rand;
        protected IClientGameState _gs;
        protected IClientEntityService _clientEntityService;
        protected IEntityService _entityService;
        private ILogService _logService;
        private IClientUpdateService _updateService;
        private CancellationToken _token;
        protected IAwaitableService _awaitableService;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public void SetGameToken(CancellationToken token)
        {
            _token = token;
        }

        public void SetText(IText itext, string txt)
        {
            _updateService.RunOnMainThread(
                () =>
                {
                    if (itext is GText gtext)
                    {
                        gtext.SetText(txt);
                    }
                });
        }

        public void SetInputText(IInputField iInput, object obj)
        {
            if (obj != null && iInput is GInputField ginput)
            {
                ginput.text = obj.ToString();
            }
        }

        public int GetIntInput(IInputField iinput)
        {
            if (iinput is GInputField ginput)
            {
                if (Int32.TryParse(ginput.text, out int value))
                {
                    return value;
                }
            }
            return 0;
        }

        public long GetSelectedIdFromName(Type iidNameType, IDropdown idropdown)
        {
            if (!(idropdown is GDropdown gdropdown))
            {
                return 0;
            }

            List<IIdName> items = _entityService.GetChildList(_gs.ch, iidNameType.Name);

            string selectedText = gdropdown.captionText.text;

            IIdName selectedItem = items.FirstOrDefault(x => x.Name == gdropdown.captionText.text);

            return selectedItem?.IdKey ?? 0;

        }

        public void SetImageTexture(IRawImage image, object texObj)
        {
            if (image is GRawImage gimage)
            {
                if (texObj is Texture tex)
                {
                    gimage.texture = tex;
                }
                else if (texObj == null)
                {
                    gimage.texture = null;
                }
            }
        }

        public object GetSelected()
        {
            if (EventSystem.current == null)
            {
                return null;
            }
            return EventSystem.current.currentSelectedGameObject;
        }


        public void SetColor(IText text, object colorObj)
        {
            if (text is GText gtext && colorObj is UnityEngine.Color color)
            {
                gtext.color = color;
            }
        }

        public void SetButton(IButton button, string screenName, Action action, Dictionary<string, string> extraData = null)
        {
            if (button is GButton gbutton)
            {
                gbutton.onClick.AddListener(
                   () =>
                   {
                       _awaitableService.ForgetAwaitable(InnerButtonClick(gbutton, screenName, action, null, extraData));

                   });
            }
        }
        public void SetButton(IButton button, string screenName, Func<CancellationToken,Task> awaitableAction, Dictionary<string, string> extraData = null)
        {
            if (button is GButton gbutton)
            {
                gbutton.onClick.AddListener(
                   () =>
                   {
                       _awaitableService.ForgetAwaitable(InnerButtonClick(gbutton, screenName, null, awaitableAction, extraData));

                   });
            }
        }

        private bool _canClickButton = true;
        private async Awaitable InnerButtonClick (GButton button, string screenName, Action action, Func<CancellationToken,Task> awaitableAction, Dictionary<string, string> extraData = null)
        {
            if (!_canClickButton)
            {
                return;
            }
            try
            {
                _canClickButton = false;
                if (button != null)
                {
                    button.interactable = false;
                }
                if (_ftueService.IsComplete(_rand, _gs.ch))
                {
                    _analyticsService.Send(AnalyticsEvents.ClickButton, button.name, screenName, extraData);
                    _audioService.PlaySound(AudioList.ButtonClick);
                    if (action != null)
                    {
                        action();
                    }
                    else if (awaitableAction != null)
                    {
                        await awaitableAction(_token);
                    }
                }
                else
                {
                    FtueStep step = _ftueService.GetCurrentStep(_rand, _gs.ch);
                    if (_ftueService.CanClickButton(_rand, _gs.ch, screenName, button.name))
                    {
                        _audioService.PlaySound(AudioList.ButtonClick);
                        _analyticsService.Send(AnalyticsEvents.ClickButton, button.name, screenName, extraData);
                        if (action != null)
                        {
                            action();
                        }
                        else if (awaitableAction != null)
                        {
                            await awaitableAction(_token);
                        }
                        if (step != null)
                        {
                            _ftueService.CompleteStep(_rand, _gs.ch, step.IdKey);
                            _realtimeNetworkService.SendMapMessage(new CompleteFtueStepMessage() { FtueStepId = step.IdKey });
                        }
                    }
                    else
                    {
                        _audioService.PlaySound(AudioList.ErrorClick);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "ButtonClick");
            }
            finally
            {
                if (button != null)
                {
                    button.interactable = true;
                }
                _canClickButton = true;
            }
        }

        public void SetAlpha(IText text, float alpha)
        { 
            if (text is GText gText)
            {
                gText.alpha = alpha;
            }
        }

        public void SetAutoSizing(IText text, bool autoScaling)
        {
            if (text is GText gtext)
            {
                gtext.enableAutoSizing = autoScaling;
            }
        }

        public void ResizeGridLayout(IGridLayoutGroup group, float xscale, float yscale)
        {
            if (group is GGridLayoutGroup ggroup)
            {
                ggroup.constraintCount = (int) (ggroup.constraintCount / xscale);
                ggroup.cellSize = new Vector2(ggroup.cellSize.x * xscale, ggroup.cellSize.y * yscale);
            }
        }

        public void AddPointerHandlers(IView view, Action enterHandler, Action exitHandler)
        {
            if (view is MonoBehaviour mb)
            {
                PointerHandler ph = _clientEntityService.GetOrAddComponent<PointerHandler>(mb.gameObject);
                ph.SetEnterExitHandlers(enterHandler, exitHandler);
            }
        }

        public void ScrollToBottom(object scrollRectObj)
        {
            if (scrollRectObj is ScrollRect scrollRect)
            {
                scrollRect.normalizedPosition = new Vector2(0, 0);
            }
        }
        public void ScrollToTop(object scrollRectObj)
        {
            if (scrollRectObj is ScrollRect scrollRect)
            {
                scrollRect.normalizedPosition = new Vector2(0, 1);
            }
        }

        public void SetTextAlignemnt(IText text, int offset)
        {
            if (text is GText gtext)
            {
                gtext.alignment = (offset < 0 ? TMPro.TextAlignmentOptions.Left : offset > 0 ? TMPro.TextAlignmentOptions.Right : TMPro.TextAlignmentOptions.Center);
            }
        }

        public object GetImageTexture(IRawImage image)
        {
            if (image is GRawImage gimage)
            {
                return gimage.texture;
            }
            return null;
        }

        public int GetImageHeight(IRawImage image)
        {
            if (image is GRawImage gimage)
            {
                if (gimage.texture != null)
                {
                    return gimage.texture.height;
                }
            }
            return 0;
        }

        public int GetImageWidth(IRawImage image)
        {
            if (image is GRawImage gimage)
            {
                if (gimage.texture != null)
                {
                    return gimage.texture.width;
                }
            }
            return 0;
        }

        public void SetUVRect(IRawImage image, float xpos, float ypos, float xsize, float ysize)
        {
            if (image is GRawImage gimage)
            {
                gimage.uvRect = new UnityEngine.Rect(new Vector2(xpos,ypos), new Vector2(xsize,ysize));
            }
        }
    }
}
