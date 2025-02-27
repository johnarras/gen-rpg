using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Units.Interfaces
{
    public interface IUnitRole : IIndexedGameItem
    {
        public string PluralName { get; set; }
        int MinRange { get; set; }
        long MinLevel { get; set; }
        List<UnitEffect> Effects { get; set; }
    }
}
