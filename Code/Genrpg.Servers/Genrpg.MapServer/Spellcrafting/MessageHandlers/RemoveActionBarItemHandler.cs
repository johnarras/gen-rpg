using Genrpg.MapServer.MapMessaging.MessageHandlers;
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
    public class RemoveActionBarItemHandler : BaseServerMapMessageHandler<RemoveActionBarItem>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, RemoveActionBarItem message)
        {
            if (!_objectManager.GetChar(obj.Id, out Character ch))
            {
                return;
            }

            ActionInputData actionData = ch.Get<ActionInputData>();

            ActionInput input = actionData.GetInput(message.Index);

            if (input != null)
            {
                actionData.SetInput(message.Index, 0, _repoService);
                ch.AddMessage(new OnRemoveActionBarItem() { Index = message.Index });
            }
        }
    }
}
