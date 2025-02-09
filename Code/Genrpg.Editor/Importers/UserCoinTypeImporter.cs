using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.UserCoins.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers
{
    public class UserCoinTypeImporter : ParentChildImporter<UserCoinSettings, UserCoinType>
    {
        public override string ImportDataFilename => "UserCoinImport.csv";

        public override EImportTypes GetKey() { return EImportTypes.UserCoins; }

        protected override void ImportChildSubObject(EditorGameState gs, UserCoinType current, string[] headers, string[] rowWords)
        {
        }
    }
}
