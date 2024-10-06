using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Utils;
using Genrpg.Shared.Crawler.Spells.Settings;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers
{

    public class CrawlerSpellImporter : BaseCrawlerDataImporter
    {
        public override string ImportDataFilename => "CrawlerSpellImport.csv";

        public override EImportTypes GetKey() { return EImportTypes.CrawlerSpells; }

        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, string[] lines)
        {
            string[] spellHeaderLine = lines[0].Split(',');
            string[] effectHeaders= lines[1].Split(",");    

            CrawlerSpellSettings settings = gs.data.Get<CrawlerSpellSettings>(null);
            List<CrawlerSpell> crawlerSpells = settings.GetData().ToList();

            CrawlerSpell currentSpell = null;
            for (int l = 2; l < lines.Length; l++)
            {
                string[] words = lines[l].Split(",");

                if (words[0] == "spell")
                {
                    if (words.Length < 3)
                    {
                        continue;
                    }

                    if (!Int64.TryParse(words[1], out long currIdKey))
                    {
                        continue;
                    }

                    CrawlerSpell oldSpell = crawlerSpells.FirstOrDefault(x => x.IdKey == currIdKey);

                    currentSpell = DataImportTools.ImportLine<CrawlerSpell>(words, spellHeaderLine, oldSpell);

                    if (oldSpell == null)
                    {
                        CrawlerSpell existingSpell = crawlerSpells.FirstOrDefault(X => X.IdKey == currentSpell.IdKey);
                        if (existingSpell != null)
                        {
                            throw new Exception("Duplicated spell Idkey in import");
                        }
                        crawlerSpells.Add(currentSpell);
                        currentSpell.Effects = new List<CrawlerSpellEffect>();
                    }
                    currentSpell.Effects = new List<CrawlerSpellEffect>();
                }
                else if (words[0] == "effect")
                {
                    CrawlerSpellEffect effect = DataImportTools.ImportLine<CrawlerSpellEffect>(words, effectHeaders);
                    currentSpell.Effects.Add(effect);
                }
            }
            settings.SetData(crawlerSpells);
            foreach (CrawlerSpell crawlerSpell in crawlerSpells)
            {
                gs.LookedAtObjects.Add(crawlerSpell);
            }

            await Task.CompletedTask;
            return true;
        }
    }
}