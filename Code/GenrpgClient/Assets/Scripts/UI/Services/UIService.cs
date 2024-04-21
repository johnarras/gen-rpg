using Assets.Scripts.Interfaces;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Ftue.Messages;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Ftue.Settings;
using Genrpg.Shared.Ftue.Settings.Steps;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Utils;
using Genrpg.Shared.Interfaces;
using Scripts.Assets.Audio.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static System.Collections.Specialized.BitVector32;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.UI.Services
{
    public class UIInitializable : IUIInitializable
    {
        protected IFtueService _ftueService;
        protected IAudioService _audioService;
        protected IRealtimeNetworkService _realtimeNetworkService;
        protected IAnalyticsService _analyticsService;
        protected IGameData _gameData;

        protected UnityGameState _gs;

        public void SetText(UnityGameState gs, GText gtext, string txt)
        {
            _gs = gs;
        }

        public async Task Initialize(GameState gs, CancellationToken token)
        {
            _gs = gs as UnityGameState;
            await Task.CompletedTask;
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
            if (button == null || action == null)
            {
                return;
            }

            if (_ftueService.IsComplete(_gs, _gs.ch))
            {
                button.onClick.AddListener(
                   () =>
                   {
                       _analyticsService.Send(_gs, AnalyticsEvents.ClickButton, button.name, screenName, extraData); 
                       _audioService.PlaySound(_gs, AudioList.ButtonClick);
                       action();
                   });

            }
            else
            {
                button.onClick.AddListener(
                   () =>
                   {
                       FtueStep step = _ftueService.GetCurrentStep(_gs, _gs.ch);
                       if (_ftueService.CanClickButton(_gs, _gs.ch, screenName, button.name))
                       {
                           _audioService.PlaySound(_gs, AudioList.ButtonClick);

                           _analyticsService.Send(_gs, AnalyticsEvents.ClickButton, button.name, screenName, extraData);
                           action();

                           if (step != null)
                           {
                               _ftueService.CompleteStep(_gs, _gs.ch, step.IdKey);
                               _realtimeNetworkService.SendMapMessage(new CompleteFtueStepMessage() { FtueStepId = step.IdKey });
                           }                            
                       }
                       else
                       {
                           _audioService.PlaySound(_gs, AudioList.ErrorClick);
                       }
                   });
            }

        }

        public void AddEventListener(UnityGameState gs, GEntity go, EventTriggerType type, UnityAction<BaseEventData> callback)
        {
            EventTrigger trigger = GEntityUtils.GetOrAddComponent<EventTrigger>(_gs, go);
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener(callback);
        }
    }
}
