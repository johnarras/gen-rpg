using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.SpellCrafting.Services;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.MapServer.Spellcrafting.MessageHandlers
{
    public class CraftSpellHandler : BaseCharacterServerMapMessageHandler<CraftSpell>
    {
        private ISharedSpellCraftService _spellCraftService = null;

        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, CraftSpell message)
        {
            Spell spell = message.CraftedSpell;

            if (spell == null)
            {
                return;
            }

            spell.OwnerId = ch.Id;
            if (string.IsNullOrEmpty(spell.Id))
            {
                spell.Id = HashUtils.NewGuid();
            }

            if (!_spellCraftService.ValidateSpellData(ch, spell))
            {
                ch.AddMessage(new ErrorMessage("Failed to craft spell!"));
            }

            SpellData spellData = ch.Get<SpellData>();

            long maxId = 0;

            if (spellData.GetData().Count > 0)
            {
                maxId = spellData.GetData().Max(x => x.IdKey);
            }

            if (spell.IdKey < 1)
            {
                spell.IdKey = ++maxId;
            }

            spellData.Add(spell);
            _repoService.QueueSave(spell);
            ch.AddMessage(new OnCraftSpell() { CraftedSpell = spell });
        }
    }
}
