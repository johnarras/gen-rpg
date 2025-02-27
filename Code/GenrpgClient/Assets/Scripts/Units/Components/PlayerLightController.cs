
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Utils;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class PlayerLightController : BaseBehaviour
    {
        private IModTextureService _modTextureService;
        private ICrawlerMapService _crawlerMapService;
        private ICrawlerService _crawlerService;
        private ICrawlerWorldService _crawlerWorldService;

        public float Range = 75;

        public Light Headlight;

        float _currIntensity = 0;
        float _targetIntensity = 0;

        const float IntensityDelta = 10f;

        public float MaxIntensity = 150;

        public Vector3 Offset;

        private float _startMaxIntensity;

        private int _maxStableTicks = 5;
        private int _stableTicksLeft = 0;

        public override void Init()
        {
            base.Init();
            AddUpdate(LightUpdate, UpdateTypes.Late);
            _startMaxIntensity = MaxIntensity;
            _targetIntensity = MaxIntensity;
            _currIntensity = MaxIntensity;
        }

        bool haveSetPosition = false;
        private void LightUpdate()
        {
            if (!_crawlerMapService.IsIndoors())
            {
                return;
            }

            PartyData partyData = _crawlerService.GetParty();
            CrawlerMap map = _crawlerWorldService.GetMap(partyData.MapId);


            if (_crawlerMapService.HasMagicBit(partyData.MapX, partyData.MapZ, MapMagic.Darkness))
            {
                Headlight.intensity = 0;
                return;
            }

            if (!haveSetPosition)
            {
               entity.transform.localPosition = Offset;
            }
            haveSetPosition = true;

            if (_currIntensity != _targetIntensity)
            {
                _currIntensity = _modTextureService.MoveCurrFloatToTarget(_currIntensity, _targetIntensity, MathUtils.FloatRange(0, IntensityDelta * 2, _rand));
            }

            if (_currIntensity == _targetIntensity)
            {
                _stableTicksLeft--;

                if (_stableTicksLeft <= 0)
                {
                    _targetIntensity = MathUtils.FloatRange(_startMaxIntensity * 2 / 3, _startMaxIntensity,_rand);
                    _stableTicksLeft = MathUtils.IntRange(0, _maxStableTicks, _rand);
                }
            }

            if (Headlight != null)
            {
                Headlight.intensity = _currIntensity * partyData.Buffs.Get(PartyBuffs.Light);
                Headlight.range = Range;
            }
        }
    }
}
