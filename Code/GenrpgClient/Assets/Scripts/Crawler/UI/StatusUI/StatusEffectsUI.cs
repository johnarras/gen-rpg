using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.StatusUI
{
    public class StatusEffectsUI : BaseBehaviour
    {

        public GameObject IconAnchor;

        public StatusEffectIcon IconPrefab;

        private List<StatusEffectIcon> _effectIcons = new List<StatusEffectIcon>();

        private long _currentStatusEffects = 0;
        public void InitData(Unit unit)
        {
            long newStatusEffects = unit.StatusEffects.Bits[0];

            if (newStatusEffects == _currentStatusEffects)
            {
                return;
            }

            IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

            List<StatusEffectIcon> removeList = new List<StatusEffectIcon>();

            foreach (StatusEffectIcon effectIcon in _effectIcons)
            {
                if (!unit.StatusEffects.HasBit(effectIcon.GetStatusEffectId()))
                {
                    removeList.Add(effectIcon);
                }
            }

            foreach (StatusEffectIcon effectIcon in removeList)
            {
                _clientEntityService.Destroy(effectIcon.gameObject);
                _effectIcons.Remove(effectIcon);
            }

            foreach (StatusEffect effect in effects)
            {
                if (unit.StatusEffects.HasBit(effect.IdKey))
                {
                    if (!_effectIcons.Any(x=>x.GetStatusEffectId() == effect.IdKey))
                    {
                        StatusEffectIcon newIcon = _clientEntityService.FullInstantiate<StatusEffectIcon>(IconPrefab);

                        _effectIcons.Add(newIcon);
                        newIcon.InitData(effect.IdKey);
                    }
                }
            }
        }
    }
}
