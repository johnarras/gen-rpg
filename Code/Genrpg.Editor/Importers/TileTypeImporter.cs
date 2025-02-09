using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Tiles.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers
{
    public class TileTypeImporter : ParentChildImporter<TileTypeSettings, TileType>
    {
        public override string ImportDataFilename => "TileTypeImport.csv";

        public override EImportTypes GetKey() { return EImportTypes.TileTypes; }

        protected override void ImportChildSubObject(EditorGameState gs, TileType current, string[] headers, string[] rowWords)
        {
        }
    }
}
