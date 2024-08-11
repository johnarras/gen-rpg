using Genrpg.Shared.GameSettings;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GameSettings.Entities
{
    public class ClientGameData : GameData
    {
        private IFilteredObject _obj = null;
        public override T Get<T>(IFilteredObject obj)
        {
            if (obj == null)
            {
                obj = _obj;
            }
            return base.Get<T>(obj);
        }

        public void SetFilteredObject(IFilteredObject obj)
        {
            _obj = null;
        }
    }
}
