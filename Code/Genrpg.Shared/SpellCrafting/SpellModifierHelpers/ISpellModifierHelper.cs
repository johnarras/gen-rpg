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
        double GetMinValue(GameState gs, MapObject obj);
        double GetMaxValue(GameState gs, MapObject obj);
        double GetCostScale(GameState gs, MapObject obj, double currValue);
        double GetValidValue(GameState gs, MapObject obj, double currValue);
        string GetInfoText(GameState gs, MapObject obj);
        List<double> GetValidValues(GameState gs, MapObject obj);
    }
}
