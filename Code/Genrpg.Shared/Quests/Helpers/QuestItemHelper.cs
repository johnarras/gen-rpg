using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.ProcGen.Settings.Names;
using System.Collections.Generic;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Quests.WorldData;
namespace Genrpg.Shared.Quests.Helpers
{
    public class QuestItemHelper : BaseMapEntityHelper<QuestItem>
    {
        public override long GetKey() { return EntityTypes.QuestItem; }
    }
}
