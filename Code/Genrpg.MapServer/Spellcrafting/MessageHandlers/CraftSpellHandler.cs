using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Spells.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.MapServer.Spellcrafting.MessageHandlers
{
    public class CraftSpellHandler : BaseServerMapMessageHandler<CraftSpell>
    {
        private ISharedSpellCraftService _spellCraftService;

        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, CraftSpell message)
        {

            if (!_objectManager.GetChar(obj.Id, out Character ch))
            {
                return;
            }

            Spell spell = message.CraftedSpell;

            if (spell == null)
            {
                return;
            }

            if (!_spellCraftService.GenerateSpellData(gs, spell))
            {
                obj.AddMessage(new ErrorMessage("Failed to craft spell!"));
            }

            SpellData spellData = ch.Get<SpellData>();

            long maxId = 0;

            if (spellData.Data.Count > 0)
            {
                maxId = spellData.Data.Max(x => x.IdKey);
            }

            if (spell.IdKey < 1)
            {
                spell.IdKey = ++maxId;
            }

            spellData.Add(spell);
            ch.AddMessage(new OnCraftSpell() { CraftedSpell = spell });
            spellData.SetDirty(true);
        }
    }
}
