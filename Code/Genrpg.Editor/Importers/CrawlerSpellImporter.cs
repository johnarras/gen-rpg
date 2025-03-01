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

        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, List<string[]> lines)
        {
            string[] spellHeaderLine = lines[0];
            string[] effectHeaders= lines[1];    

            CrawlerSpellSettings spellSettings = gs.data.Get<CrawlerSpellSettings>(null);
            List<CrawlerSpell> crawlerSpells = new List<CrawlerSpell>();


            List<CrawlerSpell> oldSpells = spellSettings.GetData().ToList();

            long currSpellId = 0;
            CrawlerSpell currentSpell = null;
            for (int l = 2; l < lines.Count; l++)
            {
                string[] words = lines[l];

                if (words[0] == "spell")
                {
                    if (words.Length < 3)
                    {
                        continue;
                    }

                    currentSpell = _importService.ImportLine<CrawlerSpell>(gs, l, words, spellHeaderLine);

                    currentSpell.IdKey = ++currSpellId;

                    crawlerSpells.Add(currentSpell);
                   
                    currentSpell.Effects = new List<CrawlerSpellEffect>();
                }
                else if (words[0] == "effect")
                {
                    CrawlerSpellEffect effect = _importService.ImportLine<CrawlerSpellEffect>(gs, l, words, effectHeaders);
                    currentSpell.Effects.Add(effect);
                }
            }
            foreach (CrawlerSpell crawlerSpell in crawlerSpells)
            {
                gs.LookedAtObjects.Add(crawlerSpell);
            }
            spellSettings.SetData(crawlerSpells);

            await Task.CompletedTask;
            return true;
        }
    }
}