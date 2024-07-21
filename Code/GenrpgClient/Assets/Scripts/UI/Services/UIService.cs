using Assets.Scripts.Interfaces;
using Assets.Scripts.ProcGen.RandomNumbers;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Ftue.Messages;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Ftue.Settings.Steps;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Scripts.Assets.Audio.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.UI.Services
{
    public class UIInitializable : IUIService
    {
        protected IFtueService _ftueService;
        protected IAudioService _audioService;
        protected IRealtimeNetworkService _realtimeNetworkService;
        protected IAnalyticsService _analyticsService;
        protected IGameData _gameData;
        protected IClientRandom _rand;
        protected IUnityGameState _gs;
        protected IGameObjectService _gameObjectService;
        private ILogService _logService;
        private CancellationToken _token;
        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public void SetGameToken(CancellationToken token)
        {
            _token = token;
        }

        public void SetText(GText gtext, string txt)
        {
            if (gtext != null)
            {
                gtext.text = txt;
            }
        }

        public void SetInputText(GInputField input, object obj)
        {
            if (input != null && obj != null)
            {
                input.text = obj.ToString();
            }
        }

        public int GetIntInput(GInputField field)
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

        public long GetSelectedIdFromName(Type iidNameType, GDropdown dropdown)
        {
            List<IIdName> items = GameDataUtils.GetIdNameList(_gameData, iidNameType.Name);

            string selectedText = dropdown.captionText.text;

            IIdName selectedItem = items.FirstOrDefault(x => x.Name == dropdown.captionText.text);

            return selectedItem?.IdKey ?? 0;

        }

        public void AddEventListener(GEntity go, EventTriggerType type, UnityAction<BaseEventData> callback)
        {
            EventTrigger trigger = _gameObjectService.GetOrAddComponent<EventTrigger>(go);
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener(callback);
        }

        public void SetImageTexture(GRawImage image, Texture tex)
        {
            if (image == null)
            {
                return;
            }

            image.texture = tex;
        }

        public GEntity GetSelected()
        {
            if (EventSystem.current == null)
            {
                return null;
            }
            return EventSystem.current.currentSelectedGameObject;
        }


        public void SetColor(GText text, UnityEngine.Color color)
        {
            if (text == null)
            {
                return;
            }

            text.color = color;
        }


        public void SetButton(GButton button, string screenName, UnityAction action, Dictionary<string, string> extraData = null)
        {
            button.onClick.AddListener(
               () =>
               {
                   AwaitableUtils.ForgetAwaitable(InnerButtonClick(button, screenName, action, null, extraData));

               });
        }
        public void SetButton(GButton button, string screenName, Func<CancellationToken,Awaitable> awaitableAction, Dictionary<string, string> extraData = null)
        {
            button.onClick.AddListener(
               () =>
               {
                   AwaitableUtils.ForgetAwaitable(InnerButtonClick(button, screenName, null, awaitableAction, extraData));

               });
        }

        private bool _canClickButton = true;
        private async Awaitable InnerButtonClick (GButton button, string screenName, UnityAction action, Func<CancellationToken,Awaitable> awaitableAction, Dictionary<string, string> extraData = null)
        {
            if (!_canClickButton)
            {
                return;
            }
            try
            {
                _canClickButton = false;
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
                _canClickButton = true;
            }
        }
    }
}
