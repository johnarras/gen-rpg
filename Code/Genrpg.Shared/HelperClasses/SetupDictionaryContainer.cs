using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.HelperClasses
{
    public class SetupDictionaryContainer<Key, Val> : IInitOnResolve where Val : ISetupDictionaryItem<Key>
    {
        private IServiceLocator _loc = null!;
        private Dictionary<Key, Val> _dictionary = new Dictionary<Key, Val>();
        public void Init()
        {
            _dictionary = ReflectionUtils.SetupDictionary<Key, Val>(_loc);
        }

        public bool TryGetValue(Key key, out Val value)
        {
            if (_dictionary.TryGetValue(key, out value))
            {
                return true;
            }
            return false;
        }

        public Dictionary<Key, Val> GetDict()
        {
            return _dictionary;
        }
    }
}
