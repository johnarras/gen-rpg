using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.UI;
using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Stats.Settings.Stats;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Genrpg.Editor.Importers
{
    public class CrawlerClassImporter : BaseDataImporter
    {
        public override string ImportDataFilename => "CrawlerClassImport.csv";

        public override EImportTypes GetKey() { return EImportTypes.CrawlerClasses; }

        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, string[] lines)
        {
            string[] firstLine = lines[0].Split(',');

            string missingWords = "";
            IReadOnlyList<Class> classes = gs.data.Get<ClassSettings>(null).GetData();

            int maxClasses = 50;
            Class[] topRow = new Class[maxClasses];

            for (int s = 0; s < firstLine.Length; s++)
            {
                topRow[s] = classes.FirstOrDefault(x => x.Name == firstLine[s].Trim());
                if (topRow[s] != null)
                {
                    gs.LookedAtObjects.Add(topRow[s]);
                    topRow[s].Bonuses = new List<ClassBonus>();
                }
            }

            IReadOnlyList<PartyBuff> partyBuffs = gs.data.Get<PartyBuffSettings>(null).GetData();

            IReadOnlyList<CrawlerSpell> crawlerSpells = gs.data.Get<CrawlerSpellSettings>(null).GetData();

            IReadOnlyList<StatType> statTypes = gs.data.Get<StatSettings>(null).GetData();

            PropertyInfo[] props = typeof(Class).GetProperties();

            for (int line = 1; line < lines.Length; line++)
            {
                string[] words = lines[line].Split(',');

                if (words.Length < 2 || string.IsNullOrEmpty(words[0].Trim()))
                {
                    continue;
                }

                PartyBuff partyBuff = partyBuffs.FirstOrDefault(x => x.Name == words[0]);

                if (partyBuff != null)
                {
                    for (int w = 1; w < words.Length && w < maxClasses; w++)
                    {
                        if (topRow[w] != null && !string.IsNullOrEmpty(words[w]))
                        {
                            topRow[w].Bonuses.Add(new ClassBonus() { EntityTypeId = EntityTypes.PartyBuff, EntityId = partyBuff.IdKey, Quantity = 1 });
                        }
                    }
                    continue;
                }

                StatType statType = statTypes.FirstOrDefault(x => x.Name == words[0].Trim());

                if (statType != null)
                {
                    for (int w = 1; w < words.Length && w < maxClasses; w++)
                    {
                        if (topRow[w] != null && !string.IsNullOrEmpty(words[w]))
                        {
                            topRow[w].Bonuses.Add(new ClassBonus() { EntityTypeId = EntityTypes.Stat, EntityId = statType.IdKey, Quantity = 1 });
                        }
                    }
                    continue;
                }

                CrawlerSpell crawlerSpell = crawlerSpells.FirstOrDefault(x => x.Name == words[0]);


                if (crawlerSpell != null)
                {
                    for (int w = 1; w < words.Length && w < maxClasses; w++)
                    {
                        if (topRow[w] != null && !string.IsNullOrEmpty(words[w]))
                        {
                            topRow[w].Bonuses.Add(new ClassBonus() { EntityTypeId = EntityTypes.CrawlerSpell, EntityId = crawlerSpell.IdKey, Quantity = 1 });
                        }
                    }
                    continue;
                }


                PropertyInfo prop = props.FirstOrDefault(x => x.Name == words[0]);

                if (prop != null)
                {
                    for (int w = 0; w < words.Length && w < maxClasses; w++)
                    {
                        if (topRow[w] != null && !String.IsNullOrEmpty(words[w]))
                        {
                            if (Int32.TryParse(words[w], out int val))
                            {
                                EntityUtils.SetObjectValue(topRow[w], prop, val);
                            }
                        }
                    }
                    continue;
                }


                if (words[0].ToLower() != "count")
                {
                    missingWords += words[0] + " -- ";
                }
            }

            if (!string.IsNullOrWhiteSpace(missingWords))
            {
                ContentDialogResult result2 = await UIHelper.ShowMessageBox(window, "Bad Import Data:"  + missingWords);
                return false;
            }

            for (int c = 0; c < topRow.Length; c++)
            {
                if (topRow[c] != null)
                {
                    topRow[c].Bonuses = topRow[c].Bonuses.OrderBy(x=>x.EntityTypeId).ThenBy(x=>x.EntityId).ToList();    
                }
            }

            await Task.CompletedTask;
            return true;
        }
    }
}
