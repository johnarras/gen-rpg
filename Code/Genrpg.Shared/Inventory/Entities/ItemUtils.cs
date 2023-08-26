
using Genrpg.Shared.Constants;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crafting.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Levels.Entities;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Genrpg.Shared.Inventory.Entities
{
    public static class ItemUtils
    {
        public static string GetName(GameState gs, Item item)
        {
            if (!string.IsNullOrEmpty(item.Name))
            {
                return item.Name;
            }
            ItemType itype = gs.data.GetGameData<ItemSettings>().GetItemType(item.ItemTypeId);
            if (itype == null)
            {
                return "Item";
            }

            item.Name = itype.Name;
            if (item.Name == RecipeType.RecipeItemName)
            {
                ItemEffect firstSet = item.Effects.FirstOrDefault(X => X.EntityTypeId == EntityType.Set);
                if (firstSet != null)
                {
                    RecipeType rtype = gs.data.GetGameData<CraftingSettings>().GetRecipeType(firstSet.EntityId);
                    if (rtype != null)
                    {
                        ScalingType stype = gs.data.GetGameData<ItemSettings>().GetScalingType(item.ScalingTypeId);
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

        public static string GetIcon(GameState gs, Item item)
        {
            if (!string.IsNullOrEmpty(item.GetIcon()))
            {
                return item.GetIcon();
            }

            ScalingType scalingType = gs.data.GetGameData<ItemSettings>().GetScalingType(item.ScalingTypeId);
            string scalingName = "";
            if (scalingType != null)
            {
                scalingName = scalingType.Icon;
            }

            string endMainName = "";

            int maxIconIndex = 1;

            bool didSetEndPrefix = false;

            string startMainName = "";

            ItemType itype = gs.data.GetGameData<ItemSettings>().GetItemType(item.ItemTypeId);
            if (itype == null || string.IsNullOrEmpty(itype.Icon))
            {
                startMainName = RpgConstants.DefaultItemIcon;
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


            int IdHash = 1;

            if (!string.IsNullOrEmpty(item.Id))
            {
                for (int c = 0; c < item.Id.Length; c++)
                {
                    IdHash += item.Id[c] * (c + 1) * (c + 1) * 17;
                }
            }

            int iconIndex = (IdHash * 131 + 29) % maxIconIndex + 1;

            if (FlagUtils.IsSet(itype.Flags, ItemType.SkipScalingIconName))
            {
                scalingName = "";
            }

            item.SetIcon(startMainName + scalingName + "_" + iconIndex.ToString("D3"));


            return item.GetIcon();
        }

        public static string GetBasicInfo(GameState gs, Item item)
        {
            if (!string.IsNullOrEmpty(item.GetBasicInfo()))
            {
                return item.GetBasicInfo();
            }

            ItemType itype = gs.data.GetGameData<ItemSettings>().GetItemType(item.ItemTypeId);
            QualityType quality = gs.data.GetGameData<ItemSettings>().GetQualityType(item.QualityTypeId);
            ScalingType scaling = gs.data.GetGameData<ItemSettings>().GetScalingType(item.ScalingTypeId);

            
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

        public static string GetMapArt(GameState gs, Item item)
        {
            if (!string.IsNullOrEmpty(item.GetArt()))
            {
                return item.GetArt();
            }

            ItemType itype = gs.data.GetGameData<ItemSettings>().GetItemType(item.ItemTypeId);
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

        public static string PrintData(GameState gs, Item item)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("IDLQN: " + item.ItemTypeId + " " + item.Level + " " + item.QualityTypeId + " " + item.Name
                + " Use: " + item.UseEntityTypeId + " " + item.UseEntityId + " ");
            if (item.Effects != null)
            {
                foreach (ItemEffect eff in item.Effects)
                {
                    string ename = "ET" + eff.EntityTypeId;
                    if (eff.EntityTypeId == EntityType.Stat || eff.EntityTypeId == EntityType.StatPct)
                    {
                        StatType stype = gs.data.GetGameData<StatSettings>().GetStatType(eff.EntityId);
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

        public static long GetBuyFromVendorPrice(GameState gs, Item item)
        {
            long buyPrice = 0;

            if (buyPrice < 1)
            {
                buyPrice = (long)(GetSellToVendorPrice(gs, item) * Math.Max(2.0f,
                    gs.data.GetGameData<VendorSettings>().BuyFromVendorPriceMult));
            }

            return buyPrice;
        }

        public static long GetSellToVendorPrice(GameState gs, Item item)
        {
            long sellPrice = 0;
            int minSellPrice = 8;
            if (sellPrice < 1)
            {
                long itemValue = minSellPrice;
                LevelData levelData = gs.data.GetGameData<LevelSettings>().GetLevel(item.Level);
                if (levelData != null)
                {
                    itemValue = levelData.KillMoney * 3;
                }

                if (itemValue < sellPrice)
                {
                    itemValue = sellPrice;
                }

                QualityType quality = gs.data.GetGameData<ItemSettings>().GetQualityType(item.QualityTypeId);
                if (quality != null && quality.ItemCostPct > 0)
                {
                    itemValue = itemValue * quality.ItemCostPct / 100;
                }
                else
                {
                    itemValue *= 100;
                }

                ScalingType scaling = gs.data.GetGameData<ItemSettings>().GetScalingType(item.ScalingTypeId);
                if (scaling != null)
                {
                    itemValue *= scaling.CostPct;
                }
                else
                {
                    itemValue *= 100;
                }

                itemValue /= 10000;

                if (itemValue < minSellPrice)
                {
                    itemValue = minSellPrice;
                }

                if (item.Quantity > 1)
                {
                    itemValue *= item.Quantity;
                }

                sellPrice = itemValue;

            }

            return sellPrice;
        }

        public static void CopyStatsFrom(Item fromItem, Item toItem)
        {
            toItem.SetArt(fromItem.GetArt());
            toItem.Quantity = fromItem.Quantity;
            toItem.Effects = fromItem.Effects;

        }
    }
}
