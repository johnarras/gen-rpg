using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spells.Settings.Effects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class SepllEffectHandler : BaseServerMapMessageHandler<ActiveSpellEffect>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, ActiveSpellEffect message)
        {
            _spellService.ApplyOneEffect(gs, message);
        }
    }
}
