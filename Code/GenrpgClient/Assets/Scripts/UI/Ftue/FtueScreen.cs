using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Genrpg.Shared.Ftue.Constants;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Ftue.Settings.Steps;
using System.Data.Common;
using System.Threading;
using UnityEngine;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.UI.Blockers
{
    public class FtueScreen : BlockerScreen
    {
        protected IFtueService _ftueService = null;

        public GEntity CircleMask;

        public GEntity DescParent;
        public GText DescText;

        public GImage BackgroundImage;
        public GButton BackgroundButton;

        protected FtueStep _step = null;

        protected override async UniTask OnStartOpen(object data, CancellationToken token)
        {

            await base.OnStartOpen(data, token);


            this._step = data as FtueStep;

            if (_step == null)
            {
                StartClose();
                return;
            }

            if (_step.FtuePopupTypeId == FtuePopupTypes.NoTintWindow)
            {
                CircleMask.SetActive(false);
            }
            else
            {
                GEntity entity = null;

                if (!string.IsNullOrEmpty(_step.ActionScreenName) && !string.IsNullOrEmpty(_step.ActionButtonName))
                {
                    ActiveScreen activeScreen = _screenService.GetScreen(_gs, _step.ActionScreenName);

                    if (activeScreen != null)
                    {
                        BaseScreen baseScreen = activeScreen.Screen as BaseScreen;

                        if (baseScreen != null)
                        {
                            entity = GEntityUtils.FindChild(baseScreen.gameObject, _step.ActionButtonName);
                        }
                    }
                }

                // Given UI 

                if (entity != null)
                {
                    RectTransform targetRect = entity.GetComponent<RectTransform>();

                    float cx = targetRect.position.x - Screen.width / 2;
                    float cy = targetRect.position.y - Screen.height / 2;

                    float sx = targetRect.rect.width / 2;
                    float sy = targetRect.rect.height / 2;

                    float sz = Mathf.Sqrt(sx * sx + sy * sy);

                    float scaleFactor = 3.0f;
                    if (CircleMask != null)
                    {
                        RectTransform maskRect = CircleMask.GetComponent<RectTransform>();
                        maskRect.position = new Vector3(cx, cy, 0);
                        maskRect.sizeDelta = new Vector2(sx * scaleFactor, sy * scaleFactor);
                    }
                    if (BackgroundImage != null)
                    {
                        GEntityUtils.SetActive(BackgroundImage, false);
                    }
                }
                else
                {
                    if (CircleMask != null)
                    {
                        RectTransform maskRect = CircleMask.GetComponent<RectTransform>();
                        maskRect.position = Vector3.zero;
                        maskRect.sizeDelta = Vector2.zero;
                        GEntityUtils.SetActive(CircleMask, false);
                    }

                    if (BackgroundImage != null)
                    {
                        BackgroundImage.raycastTarget = true;
                        GEntityUtils.SetActive(BackgroundImage, true);

                        _uIInitializable.SetButton(BackgroundButton, GetName(), OnClickBackground);
                    }
                }
            }

            if (DescParent != null && DescText != null)
            {
                if (string.IsNullOrEmpty(_step.Desc))
                {
                    DescParent.SetActive(false);
                }
                else
                {
                    DescParent.SetActive(true);
                    _uIInitializable.SetText(DescText, _step.Desc);
                }
            }

            await UniTask.CompletedTask;
        }

        private void OnClickBackground()
        {
            _ftueService.CompleteStep(_gs, _gs.ch, _step.IdKey);
            StartClose();
        }
    }
}
