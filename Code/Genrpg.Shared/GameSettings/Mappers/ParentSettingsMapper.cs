using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Mappers
{
    public class ParentSettingsMapper<TParent, TChild, TApi> : IGameSettingsMapper
      where TParent : ParentSettings<TChild>, new()
      where TChild : ChildSettings, new()
      where TApi : ParentSettingsApi<TParent, TChild>, new()
    {
        public virtual Type GetServerType() { return typeof(TParent); }
        public virtual bool SendToClient() { return true; }

        public virtual ITopLevelSettings MapToApi(ITopLevelSettings settings)
        {
            if (settings is TParent tparent)
            {

                TApi api = new TApi()
                {
                    ParentObj = tparent,
                    Data = tparent.GetData().ToList(),
                    Id = tparent.Id,
                };
                return api;
            }
            return settings;
        }
    }
}
