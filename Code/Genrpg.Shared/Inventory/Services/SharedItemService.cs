using Genrpg.Shared.Constants;
using Genrpg.Shared.Crafting.Settings.Recipes;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Inventory.Settings.Qualities;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.Stats.Settings.Scaling;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Inventory.Services
{
    public interface ISharedItemService : IInjectable
    {
        string GetName(IGameData gameData, Unit unit, Item item);
        string GetIcon(IGameData gameData, Unit unit, Item item);
        string GetBasicInfo(IGameData gameData, Unit unit, Item item);
        string GetMapArt(IGameData gameData, Item item);
        string PrintData(IGameData gameData, Unit unit, Item item);
        long CalcBuyCost(IGameData gameData, Unit unit, Item item);
        void CopyStatsFrom(Item fromItem, Item toItem);
    }
    public class SharedItemService : ISharedItemService
    {
        public string GetName(IGameData gameData, Unit unit, Item item)
        {
            if (!string.IsNullOrEmpty(item.Name))
            {
                return item.Name;
            }
            ItemType itype = gameData.Get<ItemTypeSettings>(unit).Get(item.ItemTypeId);
            if (itype == null)
            {
                return "Item";
            }

            item.Name = itype.Name;
            if (item.Name == RecipeType.RecipeItemName)
            {
                ItemEffect firstSet = item.Effects.FirstOrDefault(X => X.EntityTypeId == EntityTypes.Set);
                if (firstSet != null)
                {
                    RecipeType rtype = gameData.Get<RecipeSettings>(unit).Get(firstSet.EntityId);
                    if (rtype != null)
                    {
                        ScalingType stype = gameData.Get<ScalingTypeSettings>(unit).Get(item.ScalingTypeId);
                        if (stype != null && !string.IsNullOrEmpty(stype.Prefix))
                        {
                            item.Name = "Recipe: L " + item.Level + " " + stype.Prefix + " " + rtype.Name;
                        }
                        else
                        {
                            item.Name = "Recipe: L " + item.Level + " " + rtype.Name;
                        }
                    }
                }
            }
            return item.Name;
        }

        public string GetIcon(IGameData gameData, Unit unit, Item item)
        {
            if (!string.IsNullOrEmpty(item.GetIcon()))
            {
                return item.GetIcon();
            }

            ScalingType scalingType = gameData.Get<ScalingTypeSettings>(unit).Get(item.ScalingTypeId);
            string scalingName = "";
            if (scalingType != null)
            {
                scalingName = scalingType.Icon;
            }

            string endMainName = "";

            int maxIconIndex = 1;

            bool didSetEndPrefix = false;

            string startMainName = "";

            ItemType itype = gameData.Get<ItemTypeSettings>(unit).Get(item.ItemTypeId);
            if (itype == null || string.IsNullOrEmpty(itype.Icon))
            {
                startMainName = RpgConstants.DefaultItemIconItemName;
            }
            else
            {
                startMainName = itype.Icon;
            }

            // If the scaling had a start prefix, try to see if it matches something in the list.
            if (!string.IsNullOrEmpty(scalingName))
            {
                if (itype.IconCounts != null)
                {
                    NameCount iconCount = itype.IconCounts.FirstOrDefault(x => x.Name == scalingName);
                    if (iconCount != null)
                    {
                        maxIconIndex = iconCount.Count;
                        endMainName = iconCount.Name;
                        didSetEndPrefix = true;
                    }
                }
            }

            if (!didSetEndPrefix)
            {
                if (itype.IconCounts.Count > 0 && string.IsNullOrEmpty(itype.IconCounts[0].Name))
                {
                    maxIconIndex = 1;
                }
            }

            if (maxIconIndex < 1)
            {
                maxIconIndex = 1;
            }
            int IdHash = 1;

            if (!string.IsNullOrEmpty(item.Id))
            {
                for (int c = 0; c < Math.Min(3, item.Id.Length); c++)
                {
                    IdHash += item.Id[c] * (c + 1) * (c + 1) * 17;
                }
            }

            int iconIndex = ((IdHash * 131 + 29) % maxIconIndex) + 1;

            if (FlagUtils.IsSet(itype.Flags, ItemFlags.SkipScalingIconName))
            {
                scalingName = "";
            }

            item.SetIcon(startMainName + scalingName + "_" + iconIndex.ToString("D3"));


            return item.GetIcon();
        }

        public string GetBasicInfo(IGameData gameData, Unit unit, Item item)
        {
            if (!string.IsNullOrEmpty(item.GetBasicInfo()))
            {
                return item.GetBasicInfo();
            }

            ItemType itype = gameData.Get<ItemTypeSettings>(unit).Get(item.ItemTypeId);
            QualityType quality = gameData.Get<QualityTypeSettings>(unit).Get(item.QualityTypeId);
            ScalingType scaling = gameData.Get<ScalingTypeSettings>(unit).Get(item.ScalingTypeId);


            string basicInfo = "Lv. " + item.Level;
            if (quality != null)
            {
                basicInfo += " " + quality.Name;
            }

            if (scaling != null && !string.IsNullOrEmpty(scaling.Prefix))
            {
                basicInfo += " " + scaling.Prefix;
            }

            if (itype != null)
            {
                basicInfo += " " + itype.Name;
            }

            item.SetBasicInfo(basicInfo);

            return item.GetBasicInfo();

        }

        public string GetMapArt(IGameData gameData, Item item)
        {
            if (!string.IsNullOrEmpty(item.GetArt()))
            {
                return item.GetArt();
            }

            ItemType itype = gameData.Get<ItemTypeSettings>(null).Get(item.ItemTypeId);
            if (itype == null || string.IsNullOrEmpty(itype.Art))
            {
                item.SetArt(RpgConstants.DefaultMapItemArt);
            }
            else
            {
                item.SetArt(itype.Art);
            }
            return item.GetArt();
        }

        public string PrintData(IGameData gameData, Unit unit, Item item)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("IDLQN: " + item.ItemTypeId + " " + item.Level + " " + item.QualityTypeId + " " + item.Name + " ");
            if (item.Effects != null)
            {
                foreach (ItemEffect eff in item.Effects)
                {
                    string ename = "ET" + eff.EntityTypeId;
                    if (eff.EntityTypeId == EntityTypes.Stat || eff.EntityTypeId == EntityTypes.StatPct)
                    {
                        StatType stype = gameData.Get<StatSettings>(unit).Get(eff.EntityId);
                        if (stype == null)
                        {
                            ename = "Stat" + eff.EntityId;
                        }
                        else
                        {
                            ename = stype.Name;
                        }
                    }
                    sb.Append(" -- " + ename + " " + eff.Quantity);
                }
            }
            return sb.ToString();
        }

        public long CalcBuyCost(IGameData gameData, Unit unit, Item item)
        {
            long buyPrice = 0;
            int minBuyPrice = 8;
            if (buyPrice < 1)
            {
                long itemValue = minBuyPrice;
                LevelInfo levelData = gameData.Get<LevelSettings>(unit).Get(item.Level);
                if (levelData != null)
                {
                    itemValue = levelData.KillMoney * 5;
                }

                if (itemValue < buyPrice)
                {
                    itemValue = buyPrice;
                }

                QualityType quality = gameData.Get<QualityTypeSettings>(unit).Get(item.QualityTypeId);
                if (quality != null && quality.ItemCostPct > 0)
                {
                    itemValue = itemValue * quality.ItemCostPct / 100;
                }
                else
                {
                    itemValue *= 100;
                }

                ScalingType scaling = gameData.Get<ScalingTypeSettings>(null).Get(item.ScalingTypeId);
                if (scaling != null)
                {
                    itemValue *= scaling.CostPct;
                }
                else
                {
                    itemValue *= 100;
                }

                itemValue /= 10000;

                if (itemValue < minBuyPrice)
                {
                    itemValue = minBuyPrice;
                }

                if (item.Quantity > 1)
                {
                    itemValue *= item.Quantity;
                }

                buyPrice = itemValue;

            }

            return buyPrice;
        }

        public void CopyStatsFrom(Item fromItem, Item toItem)
        {
            toItem.SetArt(fromItem.GetArt());
            toItem.Quantity = fromItem.Quantity;
            toItem.Effects = fromItem.Effects;

        }
    }
}
