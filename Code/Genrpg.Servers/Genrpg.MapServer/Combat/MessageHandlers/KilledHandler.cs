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
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.MapServer.Levelup.Services;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Combat.MessageHandlers
{
    public class KilledHandler : BaseUnitServerMapMessageHandler<Killed>
    {
        private ICurrencyService _currencyService = null;
        private ILevelService _levelService = null;
        private IMapProvider _mapProvider = null;

        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, Killed message)
        {
            _aiService.EndCombat(rand, unit, message.UnitId, false);
            if (unit is Character ch)
            {
                Zone zone = _mapProvider.GetMap().Get<Zone>(message.ZoneId);
                if (zone != null)
                {
                    LevelInfo level = _gameData.Get<LevelSettings>(unit).Get(zone.Level);

                    if (level != null)
                    {
                        _currencyService.Add(ch, CurrencyTypes.Exp, level.MobExp);
                        _levelService.UpdateLevel(rand, ch);
                    }
                }
            }
        }
    }
}
