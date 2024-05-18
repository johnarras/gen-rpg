using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Mappers
{
    public abstract class NoChildSettingsMapper<TServer> : IGameSettingsMapper where TServer : NoChildSettings, new()
    {
        public virtual Type GetServerType() { return typeof(TServer); }
        public virtual bool SendToClient() { return true; }

        public virtual ITopLevelSettings MapToApi(ITopLevelSettings settings)
        {
            return settings;
        }

    }
}
