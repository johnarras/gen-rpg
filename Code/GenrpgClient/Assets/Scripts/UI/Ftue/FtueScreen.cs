using Genrpg.Shared.Ftue.Constants;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Ftue.Settings.Steps;
using Genrpg.Shared.UI.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Blockers
{
    public class FtueScreen : BlockerScreen
    {
        protected IFtueService _ftueService = null;

        public GameObject CircleMask;

        public GameObject DescParent;
        public GText DescText;

        public GImage BackgroundImage;
        public GButton BackgroundButton;

        protected FtueStep _step = null;

        protected override async Task OnStartOpen(object data, CancellationToken token)
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
                GameObject entity = null;

                if (!string.IsNullOrEmpty(_step.ActionScreenName) && !string.IsNullOrEmpty(_step.ActionButtonName))
                {
                    ActiveScreen activeScreen = _screenService.GetScreen(_step.ActionScreenName);

                    if (activeScreen != null)
                    {
                        BaseScreen baseScreen = activeScreen.Screen as BaseScreen;

                        if (baseScreen != null)
                        {
                            entity = (GameObject)_clientEntityService.FindChild(baseScreen.gameObject, _step.ActionButtonName);
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
                        _clientEntityService.SetActive(BackgroundImage, false);
                    }
                }
                else
                {
                    if (CircleMask != null)
                    {
                        RectTransform maskRect = CircleMask.GetComponent<RectTransform>();
                        maskRect.position = Vector3.zero;
                        maskRect.sizeDelta = Vector2.zero;
                        _clientEntityService.SetActive(CircleMask, false);
                    }

                    if (BackgroundImage != null)
                    {
                        BackgroundImage.raycastTarget = true;
                        _clientEntityService.SetActive(BackgroundImage, true);

                        _uiService.SetButton(BackgroundButton, GetName(), OnClickBackground);
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
                    _uiService.SetText(DescText, _step.Desc);
                }
            }

            
        }

        private void OnClickBackground()
        {
            _ftueService.CompleteStep(_rand, _gs.ch, _step.IdKey);
            StartClose();
        }
    }
}
