using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.Chests.Settings;

namespace Genrpg.Shared.Chests.Helpers
{
    public class ChestHelper : BaseEntityHelper<ChestSettings, Chest>
    {
        public override long GetKey() { return EntityTypes.Chest; }
    }
}
