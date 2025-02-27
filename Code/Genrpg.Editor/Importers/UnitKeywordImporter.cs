using Genrpg.Editor.Constants;
using Genrpg.Shared.Units.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers
{
    public class UnitKeywordImporter : BaseUnitDataImporter<UnitKeywordSettings, UnitKeyword>
    {
        public override string ImportDataFilename => "UnitKeywordImport.csv";

        public override EImportTypes GetKey() { return EImportTypes.UnitKeywords; }
    }
}
