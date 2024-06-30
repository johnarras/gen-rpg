using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers
{
    public interface ICsvImporter : ISetupDictionaryItem<EImportTypes>
    {
        string CSVFilename { get; }

        Task<bool> ImportData(EditorGameState gs);
    }
}
