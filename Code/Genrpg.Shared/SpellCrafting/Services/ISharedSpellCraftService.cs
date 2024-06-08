using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.SpellModifierHelpers;
using Genrpg.Shared.Spells.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.Services
{
    public interface ISharedSpellCraftService : IInitializable
    {
        bool ValidateSpellData(MapObject obj, ISpell spellType);
        ISpellModifierHelper GetSpellModifierHelper(long spellModifierId);
    }
}
