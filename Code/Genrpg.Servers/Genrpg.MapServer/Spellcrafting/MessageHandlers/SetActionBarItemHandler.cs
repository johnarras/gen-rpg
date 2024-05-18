using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.SpellCrafting.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Spellcrafting.MessageHandlers
{
    public class SetActionBarItemHandler : BaseServerMapMessageHandler<SetActionBarItem>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, SetActionBarItem message)
        {
            if (!_objectManager.GetChar(obj.Id, out Character ch))
            {
                return;
            }

            ActionInputData actionData = ch.Get<ActionInputData>();

            actionData.SetInput(message.Index, message.SpellId);

            ch.AddMessage(new OnSetActionBarItem() { Index = message.Index, SpellId = message.SpellId });

        }
    }
}
