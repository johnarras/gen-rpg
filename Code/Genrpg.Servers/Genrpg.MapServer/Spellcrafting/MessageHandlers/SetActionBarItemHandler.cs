using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Spellcrafting.MessageHandlers
{
    public class SetActionBarItemHandler : BaseCharacterServerMapMessageHandler<SetActionBarItem>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, SetActionBarItem message)
        {
            ActionInputData actionData = ch.Get<ActionInputData>();

            actionData.SetInput(message.Index, message.SpellId, _repoService);

            ch.AddMessage(new OnSetActionBarItem() { Index = message.Index, SpellId = message.SpellId });

        }
    }
}
