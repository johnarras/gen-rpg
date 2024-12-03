using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Riddles.Settings;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers
{
    public class AphorismImporter : BaseDataImporter
    {
        public override string ImportDataFilename => "Aphorisms.txt";

        public override EImportTypes GetKey() { return EImportTypes.Aphorisms; }

        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, string[] lines)
        {
            AphorismSettings settings = gs.data.Get<AphorismSettings>(null);


            List<Aphorism> newList = new List<Aphorism>();

            for (int line = 0; line < lines.Length; line++)
            {

                Aphorism aph = new Aphorism()
                {
                    IdKey = line + 1,
                    Desc = lines[line],
                    Name = "Aph" + (line + 1),
                };
                newList.Add(aph);
                gs.LookedAtObjects.Add(aph);
            }

            settings.SetData(newList);


            await Task.CompletedTask;
            return true;
        }
    }
}
