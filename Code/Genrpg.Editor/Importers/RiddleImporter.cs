using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Riddles.Settings;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Settings;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers
{

    public class RiddleImporter : BaseDataImporter
    {
        public override string ImportDataFilename => "Riddles.txt";

        public override EImportTypes GetKey() { return EImportTypes.Riddles; }



        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, List<string[]> lines)
        {
            RiddleSettings settings = gs.data.Get<RiddleSettings>(null);

            List<Riddle> riddles = new List<Riddle>();

            gs.LookedAtObjects.Add(settings);

            long riddleId = 0;

            
            for (int i = 0; i < lines.Count; i++)
            {
                if (i >= lines.Count)
                {
                    break;
                }

                string[] words = lines[i];

                if (StrUtils.IsEmptyLine(words))
                {
                    continue;
                }

                Riddle riddle = new Riddle()
                {
                    IdKey = ++riddleId
                };

                StringBuilder desc = new StringBuilder();
                while (i < lines.Count && StrUtils.IsEmptyLine(lines[i]))
                {
                    i++;
                }

                if (i >= lines.Count)
                {
                    break;
                }

                while (i < lines.Count && !StrUtils.IsEmptyLine(lines[i]))
                {
                    desc.Append(StrUtils.RecombineCSVLine(lines[i]) + "\n");
                    i++;
                }

                riddle.Desc = desc.ToString();

                if (i >= lines.Count)
                {
                    break;
                }

                while (i < lines.Count && StrUtils.IsEmptyLine(lines[i]))
                {
                    i++;
                }

                if (i >= lines.Count)
                {
                    break;
                }

                riddle.Name = StrUtils.RecombineCSVLine(lines[i]);

                riddles.Add(riddle);
                gs.LookedAtObjects.Add(riddle);
            }

            settings.SetData(riddles);

            await Task.CompletedTask;
            return true;
        }
    }
}
