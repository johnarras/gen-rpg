using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.StatusUI
{
    public class StatusEffectIcon : BaseBehaviour
    {



        public GImage Icon;




        private long _statusEffectId;

        public long GetStatusEffectId()
        {
            return _statusEffectId;
        }

        public void InitData(long statusEffectId)
        {

            _statusEffectId = statusEffectId;   

            StatusEffect statusEffect = _gameData.Get<StatusEffectSettings>(_gs.ch).Get(statusEffectId);

            if (Icon == null || statusEffect == null)
            {
                _clientEntityService.Destroy(gameObject);
                return;
            }


            _assetService.LoadAtlasSpriteInto(AtlasNames.SkillIcons, statusEffect.Icon, Icon, GetToken());

        }
    }
}
