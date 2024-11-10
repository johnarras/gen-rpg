
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Services;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class PlayerLightController : BaseBehaviour
    {
        private IModTextureService _modTextureService;
        private ICrawlerMapService _crawlerMapService;
        public float TargetIntensityScale = 0;

        public float Range = 75;

        public Light Headlight;

        float currentIntensity = 0;
        float targetIntensity = 0;

        const float IntensityDelta = 0.04f;

        public float MaxIntensity = 1.5f;

        public Vector3 Offset;

        public override void Init()
        {
            base.Init();
            AddUpdate(LightUpdate, UpdateType.Late);
        }

        bool haveSetPosition = false;
        private void LightUpdate()
        {
            if (_crawlerMapService.IsDungeon(CrawlerMapService.MapType))
            {
                return;
            }
            if (!haveSetPosition)
            {
               entity.transform.localPosition = Offset;
            }
            haveSetPosition = true;
            targetIntensity = MaxIntensity * (1.0f - ZoneStateController.AmbientScale);


            currentIntensity = _modTextureService.MoveCurrFloatToTarget(currentIntensity, targetIntensity, IntensityDelta);

            if (Headlight != null)
            {
                Headlight.intensity = currentIntensity;
                Headlight.range = Range;
            }

        }

    }
}
