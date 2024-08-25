using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Helpers
{
    public class BoardModeHelper : BaseEntityHelper<BoardModeSettings, BoardMode>
    {
        public override long GetKey() {  return EntityTypes.BoardMode; }
    }
}
