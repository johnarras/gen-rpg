using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spells.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class SepllEffectHandler : BaseServerMapMessageHandler<SpellEffect>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, SpellEffect message)
        {
            _spellService.ApplyOneEffect(gs, message);
        }
    }
}
