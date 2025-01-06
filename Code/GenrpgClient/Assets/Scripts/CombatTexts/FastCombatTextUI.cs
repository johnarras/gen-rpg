

using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.CombatTexts
{
    public class FastCombatTextUI : BaseBehaviour
    {

        private IClientAppService _appService;

        public float MaxElapsedSeconds = 1.0f;
        public float TextMoveSpeed = 30.0f;
        public GameObject Anchor;
        public FastCombatText DamageTemplate;
        public FastCombatText HealingTemplate;
        public FastCombatText InfoTemplate;


        private ConcurrentQueue<ShowCombatText> _textsToShow = new ConcurrentQueue<ShowCombatText>();

        private List<FastCombatText> _texts = new List<FastCombatText>();

        private long _framesPerSecond = 30;

        private string _unitId;
        public void SetUnitId(string unitId)
        {
            _unitId = unitId;
        }

        private string _groupId;
        public void SetGroupId(string groupId)
        {
            _groupId = groupId;
        }

        public override void Init()
        {
            base.Init();


            _framesPerSecond = _appService.TargetFrameRate;
            _dispatcher.AddListener<ShowCombatText>(OnShowCombatText, GetToken());

            _updateService.AddUpdate(this, OnUpdate, UpdateType.Regular, GetToken());
        }

        private void OnShowCombatText(ShowCombatText showCombatText)
        {
            if (!string.IsNullOrEmpty(_groupId))
            {
                if (_groupId != showCombatText.GroupId)
                {
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_unitId) || showCombatText.UnitId != _unitId)
                {
                    return;
                }
            }

            _textsToShow.Enqueue(showCombatText);
        }

        private void LoadNewTexts()
        {

            while (_textsToShow.TryDequeue(out ShowCombatText showCombatText))
            {

                FastCombatText prefab = null;

                if (showCombatText.TextType == ECombatTextTypes.Damage)
                {
                    prefab = DamageTemplate;
                }
                else if (showCombatText.TextType == ECombatTextTypes.Healing)
                {
                    prefab = HealingTemplate;
                }
                else if (showCombatText.TextType == ECombatTextTypes.Info)
                {
                    prefab = InfoTemplate;
                }
                else
                {
                    return;
                }

                prefab = _clientEntityService.FullInstantiate(prefab);
                _clientEntityService.AddToParent(prefab.gameObject, Anchor);
                prefab.ElapsedSeconds = 0;
                prefab.Speed = TextMoveSpeed;
                float angle = MathUtils.FloatRange(-45, 225, _rand);


                prefab.FrameDy = Mathf.Sin(angle * Mathf.PI / 180) * prefab.Speed / _framesPerSecond;
                prefab.FrameDx = Mathf.Cos(angle * Mathf.PI / 180) * prefab.Speed / _framesPerSecond;

                float startFrames = MathUtils.FloatRange(0, 20, _rand);

                prefab.transform.localPosition += new Vector3(prefab.FrameDx * startFrames, prefab.FrameDy * startFrames);

                prefab.ShowText(showCombatText.Text);
                _texts.Add(prefab);
            }
        }

        List<FastCombatText> removeList = new List<FastCombatText>();
        private void OnUpdate()
        {

            LoadNewTexts();

            removeList.Clear();
            foreach (FastCombatText text in _texts)
            {
                text.ElapsedSeconds += 1.0f / _framesPerSecond;
                if (text.ElapsedSeconds >= MaxElapsedSeconds)
                {
                    removeList.Add(text);
                    continue;
                }

                text.transform.localPosition += new Vector3(text.FrameDx,text.FrameDy, 0);
             
            }
            foreach (FastCombatText ctext in removeList)
            {
                _texts.Remove(ctext);
                _clientEntityService.Destroy(ctext.gameObject);
            }
        }

    }
}
