using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.MapServer.Spellcrafting.MessageHandlers
{
    public class DeleteSpellHandler : BaseCharacterServerMapMessageHandler<DeleteSpell>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, DeleteSpell message)
        {
            SpellData spellData = ch.Get<SpellData>();

            List<Spell> deleteSpells = spellData.GetData().Where(x => x.IdKey == message.SpellId).ToList();

            if (deleteSpells.Count < 1)
            {
                pack.SendError(ch, "Missing spell");
            }

            spellData.SetData(spellData.GetData().Where(x => x.IdKey != message.SpellId).ToList());
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
                actionData.SetInput(removeInput.Index, 0, _repoService);
             
            }

        }
    }
}
