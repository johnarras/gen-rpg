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


        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, string[] lines)
        {
            RiddleSettings settings = gs.data.Get<RiddleSettings>(null);

            List<Riddle> riddles = new List<Riddle>() { new Riddle() { Name = "None", Desc = "Emptiness" } };

            gs.LookedAtObjects.Add(settings);

            long riddleId = 0;

            
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i]) && i <lines.Length - 1)
                {
                    continue;
                }

                if (i >= lines.Length)
                {
                    break;
                }

                Riddle riddle = new Riddle()
                {
                    IdKey = ++riddleId
                };

                StringBuilder desc = new StringBuilder();

                do
                {
                    i++;
                }
                while (i < lines.Length && !string.IsNullOrEmpty(lines[i]) );

                if (i >= lines.Length)
                {
                    break;
                }

                riddle.Desc = desc.ToString();

                while (i < lines.Length && string.IsNullOrEmpty(lines[i]))
                {
                    i++;
                }

                if (i >= lines.Length)
                {
                    break;
                }

                riddle.Name = lines[i];

                riddles.Add(riddle);
                gs.LookedAtObjects.Add(riddle);
            }

            settings.SetData(riddles);

            await Task.CompletedTask;
            return true;
        }
    }
}
