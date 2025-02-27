﻿using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Interfaces;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers
{
    public interface IDataImporter : ISetupDictionaryItem<EImportTypes>
    {
        string ImportDataFilename { get; }

        Task<bool> ImportData(Window window, EditorGameState gs);
    }
}
