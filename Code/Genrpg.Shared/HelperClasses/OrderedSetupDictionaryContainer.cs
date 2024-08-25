using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.HelperClasses
{
    public class OrderedSetupDictionaryContainer<Key,Val> : SetupDictionaryContainer<Key,Val>, IInitOnResolve where Val : IOrderedSetupDictionaryItem<Key>
    {
        public IEnumerable<Val> OrderedItems() { return _dictionary.Values.OrderBy(x=>x.Order); }        
    }
}
