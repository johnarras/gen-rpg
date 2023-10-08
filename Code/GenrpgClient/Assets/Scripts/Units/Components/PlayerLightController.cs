
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class PlayerLightController : BaseBehaviour
    {
        public float TargetIntensityScale = 0;

        public float Range = 75;

        public Light Headlight;

        float currentIntensity = 0;
        float targetIntensity = 0;

        const float IntensityDelta = 0.04f;

        public float MaxIntensity = 1.5f;

        public GVector3 Offset;

        public override void Initialize(UnityGameState gs)
        {
            base.Initialize(gs);
            AddUpdate(LightUpdate, UpdateType.Late);
        }

        bool haveSetPosition = false;
        private void LightUpdate()
        {
            if (!haveSetPosition)
            {
               entity.transform().localPosition = GVector3.Create(Offset);
            }
            haveSetPosition = true;
            targetIntensity = MaxIntensity * (1.0f - ZoneStateController.AmbientScale);


            currentIntensity = TextureUtils.MoveCurrFloatToTarget(currentIntensity, targetIntensity, IntensityDelta);

            if (Headlight != null)
            {
                Headlight.intensity = currentIntensity;
                Headlight.range = Range;
            }

        }

    }
}
