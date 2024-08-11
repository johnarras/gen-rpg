using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Entities.Helpers
{
    public abstract class BaseMapEntityHelper<TObj> : IEntityHelper where TObj: IIdName
    {

        protected IMapProvider _mapProvider;

        public abstract long GetKey();

        public IIdName Find(IFilteredObject obj, long id)
        {
            if (_mapProvider.GetMap() == null ||
                _mapProvider.GetMap().Zones == null)
            {
                return null;
            }

            return _mapProvider.GetMap().GetEditorListFromEntityTypeId(GetKey()).FirstOrDefault();
        }

        public List<IIdName> GetChildList(IFilteredObject obj)
        {
            if (_mapProvider.GetMap() == null ||
                _mapProvider.GetMap().Zones == null)
            {
                return null;
            }

            return _mapProvider.GetMap().GetEditorListFromEntityTypeId(GetKey());
        }

        public string GetEditorPropertyName()
        {
            return typeof(TObj).Name;
        }

    }
}
