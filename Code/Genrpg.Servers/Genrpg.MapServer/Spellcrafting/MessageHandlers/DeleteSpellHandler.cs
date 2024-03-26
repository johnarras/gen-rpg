using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Spells.PlayerData.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.MapServer.Spellcrafting.MessageHandlers
{
    public class DeleteSpellHandler : BaseServerMapMessageHandler<DeleteSpell>
    {
        protected IRepositoryService _repoService = null;
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, DeleteSpell message)
        {
            if (!_objectManager.GetChar(obj.Id, out Character ch))
            {
                return;
            }

            SpellData spellData = ch.Get<SpellData>();

            List<Spell> deleteSpells = spellData.GetData().Where(x => x.IdKey == message.SpellId).ToList();

            if (deleteSpells.Count < 1)
            {
                pack.SendError(gs, ch, "Missing spell");
            }

            spellData.SetData(spellData.GetData().Where(x => x.IdKey != message.SpellId).ToList());
            spellData.SetDirty(true);
            foreach (Spell spell in deleteSpells)
            {
                _repoService.QueueDelete(spell);
            }
            ActionInputData actionData = ch.Get<ActionInputData>();

            List<ActionInput> removeInputs = actionData.GetData().Where(x => x.SpellId == message.SpellId).ToList();

            ch.AddMessage(new OnDeleteSpell() { SpellId = message.SpellId });
            foreach (ActionInput removeInput in removeInputs)
            {
                ch.AddMessage(new OnRemoveActionBarItem() { Index = removeInput.Index });
                actionData.SetInput(removeInput.Index, 0);
            }

        }
    }
}
