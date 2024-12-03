using Assets.Scripts.Assets.Textures;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.TextureLists.Services;
using Genrpg.Shared.UI.Interfaces;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Units.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Combat
{
    public class CrawlerCombatIcon : BaseBehaviour
    {

        private ITextureListCache _cache;

        public AnimatedTexture Icon;
        public GText Name;
        public GText Quantity;
        public CombatGroup Group;

        public void UpdateData()
        {
            if (Group == null)
            {
                return;
            }

            UnitType unitType = _gameData.Get<UnitSettings>(_gs.ch).Get(Group.UnitTypeId);

            if (unitType ==null)
            {
                return;
            }
            int okUnitCount = Group.Units.Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead)).Count();

            _uiService.SetText(Name, okUnitCount == 1 ? unitType.Name : unitType.PluralName);

            _uiService.SetText(Quantity, "(" + okUnitCount + ")");

            Icon.SetImage(unitType.Icon);

        }
    }
}
