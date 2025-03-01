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

        private IGameData _gameData;
        

        protected override async Task<bool> ParseInputFromLines(Window window, EditorGameState gs, List<string[]> lines)
        {
            string[] firstLine = lines[0];
            string[] secondLine = lines[1];

            string missingWords = "";

            Role[] topRow = new Role[MaxRoles];

            List<Role> newRoles = new List<Role>();

            long maxRoleId = 0;
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

                if (Int64.TryParse(secondLine[s], out long newRoleId))
                {
                    if (newRoleId < 1)
                    {
                        missingWords += " No Role Id Row 2 for Column " + s + " ";
                        break;
                    }

                    if (newRoles.Any(x=>x.IdKey == newRoleId))
                    {
                        missingWords += " Duplicate Role Idkey: " + newRoleId + " ";
                        break;
                    }


                    Role role = new Role() { IdKey = newRoleId, Name = roleName };
                    newRoles.Add(role);
                    topRow[s] = role;
                }
                else
                {
                    missingWords += " Role Idkey column " + s + " is not a number ";
                    break;
                }
                gs.LookedAtObjects.Add(topRow[s]);
                topRow[s].BinaryBonuses = new List<RoleBonusBinary>();
                topRow[s].AmountBonuses = new List<RoleBonusAmount>();

            }

            PropertyInfo[] props = typeof(Role).GetProperties();

            for (int line = 2; line < lines.Count; line++)
            {
                string[] words = lines[line];

                if (words.Length < 2 || string.IsNullOrEmpty(words[0].Trim()))
                {
                    continue;
                }

                if (TryAddAmountBonus<RoleScalingTypeSettings>(gs.data, EntityTypes.RoleScaling, words, topRow, "Scaling"))
                {
                    continue;
                }

                if (TryAddBinaryBonus<StatSettings>(gs.data, EntityTypes.Stat, words, topRow))
                {
                    continue;
                }
                if (TryAddBinaryBonus<CrawlerSpellSettings>(gs.data, EntityTypes.CrawlerSpell, words, topRow))
                {
                    continue;
                }
                if (TryAddBinaryBonus<ItemTypeSettings>(gs.data, EntityTypes.Item, words, topRow))
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
                    topRow[c].BinaryBonuses = topRow[c].BinaryBonuses.OrderBy(x=>x.EntityTypeId).ThenBy(x=>x.EntityId).ToList();    
                }
            }

            gs.data.Get<RoleSettings>(null).SetData(newRoles);

            await Task.CompletedTask;
            return true;
        }

        private bool TryAddAmountBonus<T>(IGameData gameData, long entityTypeId, string[] words, Role[] topRow, string removeSuffix = "") where T : ITopLevelSettings
        {

            try
            {
                List<IIdName> children = gameData.Get<T>(null).GetChildren().Cast<IIdName>().ToList();

                IIdName child = children.FirstOrDefault(x => x.Name == words[0].Replace(removeSuffix,""));

                if (child != null)
                {
                    for (int w = 1; w < words.Length && w < MaxRoles; w++)
                    {
                        if (topRow[w] != null && double.TryParse(words[w], out double amount))
                        {
                            if (amount != 0)
                            {
                                topRow[w].AmountBonuses.Add(new RoleBonusAmount() { EntityTypeId = entityTypeId, EntityId = child.IdKey, Amount = amount });
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid cast" + e.Message);
            }
            return false;
        }

        private bool TryAddBinaryBonus<T>(IGameData gameData, long entityTypeId, string[] words, Role[] topRow) where T : ITopLevelSettings
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
                            topRow[w].BinaryBonuses.Add(new RoleBonusBinary() { EntityTypeId = entityTypeId, EntityId = child.IdKey });
                        }
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid cast" + e.Message);
            }
            return false;
        }
    }
}
