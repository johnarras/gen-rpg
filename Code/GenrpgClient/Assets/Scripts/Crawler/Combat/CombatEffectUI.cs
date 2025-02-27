

using Assets.Scripts.CombatFX;
using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Interfaces;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Combat
{
    public class CombatEffectUI : BaseBehaviour
    {

        const string CombatHitPrefabSuffix = "CombatHit";

        private IClientAppService _appService;
        private IAudioService _audioService;
        private ICrawlerService _crawlerService;
        
        public GImage MainImage;
        public GImage HitImage;

        public bool IsMainImage = true;

        public int ScaleAmount = 30;

        public int MaxHitAnchorOffset = 300;

        public float HitImageSizeDelta = 0.5f;


        private int _hitImageFrame = 0;

        RectTransform _imageTransform;

        private ShowCombatText _nextText;
        private ShowCombatText _currText;
        private int _currFrame = 0;
        Vector2 _startSize = Vector2.zero;
        Vector2 _sizeDelta = Vector2.zero;

        Color _startColor = Color.white;
        Color _targetColor = new Color(1, 0.6f, 0.6f);

        private int _animationFrames;


        private CombatHit _currHit;
     

        private Dictionary<string, CombatHit> _combatHits = new Dictionary<string, CombatHit>();
        public override void Init()
        {
            base.Init();

            _imageTransform = MainImage.GetComponent<RectTransform>();

            _startSize = _imageTransform.sizeDelta;
            _sizeDelta = new Vector2(ScaleAmount, ScaleAmount);


            if (IsMainImage)
            {
                _dispatcher.AddListener<ShowCombatText>(OnShowCombatText, GetToken());
            }

            _updateService.AddUpdate(this, OnUpdate, UpdateTypes.Regular, GetToken());

            _clientEntityService.SetActive(HitImage, false);

        }

        public void OnShowCombatText(ShowCombatText text)
        {
            if (text.TextType == ECombatTextTypes.Damage)
            {
                _nextText = text;
            }
        }

        private void OnUpdate()
        {
            if (_currFrame == 0)
            {
                if (_nextText != null)
                {
                    _currText = _nextText;
                    _nextText = null;

                    _animationFrames = (int)(_appService.TargetFrameRate * _crawlerService.GetParty().CombatTextScrollDelay);

                    if (IsMainImage)
                    {

                        ElementType elementType = _gameData.Get<ElementTypeSettings>(_gs.ch).Get(_currText.ElementTypeId);
                        if (elementType != null)
                        {
                            _audioService.PlaySound(elementType.Art + "Hit");
                        }
                        if (_combatHits.ContainsKey(elementType.Art))
                        {
                            ShowCombatHitArt(_combatHits[elementType.Art]);
                        }
                        else
                        {
                            _assetService.LoadAssetInto(gameObject, AssetCategoryNames.Combat, elementType.Art + CombatHitPrefabSuffix, OnLoadCombatHit, elementType, GetToken());
                        }
                    }
                }
            }
            if (_currText == null)
            {
                return;
            }

            int midFrame = _animationFrames / 2;

            int frameDelta = Math.Abs(midFrame - _currFrame);

            float midPercent = 1.0f - frameDelta * 1.0f / midFrame;

            MainImage.color = _startColor * (1 - midPercent) + _targetColor * (midPercent);


            _imageTransform.sizeDelta = _startSize + _sizeDelta * midPercent;

            _currFrame++;
            if (_currFrame >= _animationFrames)
            {
                _currFrame = 0;
                _currText = null;

                _clientEntityService.SetActive(HitImage, false);
            }


            float hitAlpha = Math.Max(0, midPercent * 2 - 1);

            if (IsMainImage && _hitImageFrame >= 0)
            {
                _hitImageFrame++;
                if (_currHit == null || _currHit.Images.Count <= _hitImageFrame/2)
                {
                    _uiService.SetImageSprite(HitImage, null);
                    _clientEntityService.SetActive(HitImage, false);
                    _hitImageFrame = -1;
                }
                else
                {
                    _uiService.SetImageSprite(HitImage, _currHit.Images[_hitImageFrame/2]);
                }
            }
        }

        private void OnLoadCombatHit(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            CombatHit hit = go.GetComponent<CombatHit>();

            if (hit == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            ElementType etype = data as ElementType;

            if (etype == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            if (_combatHits.ContainsKey(etype.Art))
            {
                _clientEntityService.Destroy(go);
                return;
            }

            _combatHits[etype.Art] = hit;

            _clientEntityService.SetActive(hit.gameObject, false);

            ShowCombatHitArt(hit);
        }

        private void ShowCombatHitArt(CombatHit hit)
        {
            _currHit = null;
            if (HitImage == null || hit.Images.Count < 1)
            {
                return;
            }
            _targetColor = hit.TargetColor;

            _clientEntityService.SetActive(HitImage, true);

            _currHit = hit;
            _uiService.SetImageSprite(HitImage, hit.Images[0]);
            _hitImageFrame = 0;
            RectTransform rectTransform = HitImage.GetComponent<RectTransform>();

            float dx = MathUtils.FloatRange(-MaxHitAnchorOffset, MaxHitAnchorOffset, _rand);

            float dy = MathUtils.FloatRange(-MaxHitAnchorOffset, MaxHitAnchorOffset, _rand);

            float angle = 0;

            if (dx < 0)
            {
                if (dy < 0)
                {
                    angle = MathUtils.FloatRange(0, 90, _rand);
                }
                else
                {
                    angle = MathUtils.FloatRange(-90, 0, _rand);
                }
            }
            else
            {
                if (dy < 0)
                {
                    angle = MathUtils.FloatRange(90, 180, _rand);
                }
                else
                {
                    angle = MathUtils.FloatRange(180, 270, _rand);
                }
            }


            rectTransform.localPosition = new Vector3(dx, dy, 0);

            rectTransform.localEulerAngles = new Vector3(0, 0, angle);

            rectTransform.localScale = Vector3.one*MathUtils.FloatRange(1,1+HitImageSizeDelta, _rand);
        }
    }
}
