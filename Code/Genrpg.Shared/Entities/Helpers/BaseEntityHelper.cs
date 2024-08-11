using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Entities.Helpers
{
    public abstract class BaseEntityHelper<TParent,TChild> : IEntityHelper where TParent: ParentSettings<TChild> where TChild : ChildSettings, IIdName, new()
    {
        protected IGameData _gameData;
        public IIdName Find(IFilteredObject obj, long id)
        {
            return _gameData.Get<TParent>(obj).Get(id);
        }

        public List<IIdName> GetChildList(IFilteredObject obj)
        {
            return _gameData.Get<TParent>(obj).GetData().Cast<IIdName>().ToList();  
        }

        public abstract long GetKey();

        public virtual string GetEditorPropertyName() { return typeof(TChild).Name; }
    }
}
