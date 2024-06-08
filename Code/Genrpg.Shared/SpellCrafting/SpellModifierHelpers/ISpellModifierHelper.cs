using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spells.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public interface ISpellModifierHelper : ISetupDictionaryItem<long>
    {
        double GetMinValue(MapObject obj);
        double GetMaxValue(MapObject obj);
        double GetCostScale(MapObject obj, double currValue);
        double GetValidValue(MapObject obj, double currValue);
        string GetInfoText(MapObject obj);
        List<double> GetValidValues(MapObject obj);
    }
}
