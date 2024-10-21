using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.UI;
using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.SpellCrafting.SpellModifierHelpers;
using Genrpg.Shared.Stats.Settings.Stats;
using Microsoft.Extensions.Azure;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using Windows.Security.Cryptography.Core;
using Windows.UI.Popups;

namespace Genrpg.Editor.Importers
{
    public class CrawlerRoleImporter : BaseCrawlerDataImporter
    {
        public override string ImportDataFilename => "CrawlerRoleImport.csv";

        public override EImportTypes GetKey() { return EImportTypes.CrawlerRoles; }

        const int MaxRoles = 100;

        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, string[] lines)
        {
            string[] firstLine = lines[0].Split(',');

            string missingWords = "";
            IReadOnlyList<Role> roles = gs.data.Get<RoleSettings>(null).GetData();

            Role[] topRow = new Role[MaxRoles];

            long maxRoleId = 0;
            if (roles.Count > 0)
            {
                maxRoleId = roles.Max(x => x.IdKey);
            }
            for (int s = 1; s < firstLine.Length; s++)
            {

                string roleName = firstLine[s].Trim();
                if (string.IsNullOrEmpty(roleName))
                {
                    continue;
                }
                if (roleName.Length < 3)
                {
                    missingWords += "BadRoleName:(" + roleName + ")";
                    break;
                }
                topRow[s] = roles.FirstOrDefault(x => x.Name == roleName);
                if (topRow[s] == null)
                {
                    topRow[s] = new Role() { IdKey = ++maxRoleId, Name = roleName };
                }
                if (topRow[s] != null)
                {
                    gs.LookedAtObjects.Add(topRow[s]);
                    topRow[s].Bonuses = new List<RoleBonus>();
                }
            }

            PropertyInfo[] props = typeof(Role).GetProperties();

            for (int line = 1; line < lines.Length; line++)
            {
                string[] words = lines[line].Split(',');

                if (words.Length < 2 || string.IsNullOrEmpty(words[0].Trim()))
                {
                    continue;
                }

                if (TryAddBonus<PartyBuffSettings>(gs.data, EntityTypes.PartyBuff, words, topRow))
                {
                    continue;
                }
                if (TryAddBonus<StatSettings>(gs.data, EntityTypes.Stat, words, topRow))
                {
                    continue;
                }
                if (TryAddBonus<CrawlerSpellSettings>(gs.data, EntityTypes.CrawlerSpell, words, topRow))
                {
                    continue;
                }
                if (TryAddBonus<ItemTypeSettings>(gs.data, EntityTypes.Item, words, topRow))
                {
                    continue;
                }

                PropertyInfo prop = props.FirstOrDefault(x => x.Name == words[0]);

                if (prop != null)
                {
                    for (int w = 0; w < words.Length && w < MaxRoles; w++)
                    {
                        if (topRow[w] != null && !String.IsNullOrEmpty(words[w]))
                        {
                            if (Int32.TryParse(words[w], out int ival))
                            {
                                EntityUtils.SetObjectValue(topRow[w], prop, ival);
                            }
                            else if (double.TryParse(words[w], out double dval))
                            {
                                EntityUtils.SetObjectValue(topRow[w], prop, dval);
                            }
                            else if (prop.PropertyType == typeof(string))
                            {
                                EntityUtils.SetObjectValue(topRow[w], prop, words[w]);
                            }
                            else if (prop.PropertyType == typeof(bool))
                            {
                                if (bool.TryParse(words[w], out bool bval))
                                {
                                    EntityUtils.SetObjectValue(topRow[w], prop, bval);
                                }
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

            List<Role> newRoles = topRow.Where(x=>x != null).ToList();

            gs.data.Get<RoleSettings>(null).SetData(newRoles);

            await Task.CompletedTask;
            return true;
        }

        private bool TryAddBonus<T>(IGameData gameData, long entityTypeId, string[] words, Role[] topRow) where T : ITopLevelSettings
        {

            try
            {
                List<IIdName> children = gameData.Get<T>(null).GetChildren().Cast<IIdName>().ToList();

                IIdName child = children.FirstOrDefault(x => x.Name == words[0]);

                if (child != null)
                {
                    for (int w = 1; w < words.Length && w < MaxRoles; w++)
                    {
                        if (topRow[w] != null && !string.IsNullOrEmpty(words[w]))
                        {
                            topRow[w].Bonuses.Add(new RoleBonus() { EntityTypeId = entityTypeId, EntityId = child.IdKey });
                        }
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid cast");
            }
            return false;
        }
    }
}
