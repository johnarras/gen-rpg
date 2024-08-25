using Genrpg.RequestServer.BoardGame.BoardModeHelpers;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.Messages.RollDice;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Users.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.RequestServer.BoardGame.Entities
{
    public class RollDiceArgs
    {
        public CoreUserData UserData { get; set; }
        public BoardData Board { get; set; }
        public IBoardModeHelper Helper { get; set; }
        public BoardMode Mode { get; set; }

        public RollDiceResult Result { get; set; } = new RollDiceResult();

        public double PlayMult { get; set; } = BoardGameConstants.MinPlayMult;

        public bool ExitMode { get; set; }
    }
}
