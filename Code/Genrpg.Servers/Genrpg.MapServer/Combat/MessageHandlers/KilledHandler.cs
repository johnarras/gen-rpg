using Genrpg.MapServer.Levelup;
using Genrpg.MapServer.Combat.Messages;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.MapServer.Entities;

namespace Genrpg.MapServer.Combat.MessageHandlers
{
    public class KilledHandler : BaseServerMapMessageHandler<Killed>
    {
        private ICurrencyService _currencyService = null;
        private ILevelService _levelService = null;
        public override void Setup(GameState gs)
        {
            base.Setup(gs);
        }

        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, Killed message)
        {
            if (!_objectManager.GetUnit(obj.Id, out Unit unit))
            {
                return;
            }

            _aiService.EndCombat(gs, unit, message.UnitId, false);
            if (unit is Character ch)
            {
                Zone zone = gs.map.Get<Zone>(message.ZoneId);
                if (zone != null)
                {
                    LevelInfo level = _gameData.Get<LevelSettings>(obj).Get(zone.Level);

                    if (level != null)
                    {
                        _currencyService.Add(gs, ch, CurrencyTypes.Exp, level.MobExp);
                        _levelService.UpdateLevel(gs, ch);
                    }
                }
            }
        }
    }
}
