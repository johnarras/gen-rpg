using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.MapObjects.Messages;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Buildings
{
    public class MapBuilding : BaseBehaviour
    {
        public StoreSign Sign;

        private BuildingType _btype;
        private OnSpawn _spawn;
        public void Init(BuildingType btype, OnSpawn spawn, string overrideName = null)
        {
            _btype = btype;
            _spawn = spawn;
            Sign?.Init(_btype, _spawn, overrideName);
        }
    }
}
