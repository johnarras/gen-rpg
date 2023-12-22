using Genrpg.Shared.Charms.Constants;
using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.Charms.Settings;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Charms.Services
{
    public class CharmService : ICharmService
    {

        public List<PlayerCharmBonusList> CalcBonuses(GameState gs, string charmId)
        {
            string hash = charmId.Replace("0x", "");

            bool[] bits = new bool[CharmConstants.CharmIdBitLength];

            int hexBlockLength = 8;

            for (int i = 0; i < hash.Length; i += hexBlockLength)
            {
                string currHash = hash.Substring(i, hexBlockLength);

                int hexval = Int32.Parse(currHash, System.Globalization.NumberStyles.HexNumber);

                int start = 4 * i;
                int end = start + 32;
                for (int b = start; b < end; b++)
                {
                    bits[b] = ((hexval & (1 << (end - b - 1))) != 0 ? true : false);
                }
            }

            List<PlayerCharmBonusList> retval = new List<PlayerCharmBonusList>();

            List<CharmUse> charmUses = gs.data.GetGameData<CharmUseSettings>(null).GetList<CharmUse>();

            List<CharmBonus> charmBonuses = gs.data.GetGameData<CharmBonusSettings>(null).GetList<CharmBonus>();

            List<StatType> statTypes = gs.data.GetGameData<StatSettings>(null).GetList<StatType>();

            foreach (CharmUse charmUse in charmUses)
            {
                PlayerCharmBonusList list = new PlayerCharmBonusList()
                {
                    CharmUseId = charmUse.IdKey,
                };

                retval.Add(list);

                List<CharmBonus> currentBonuses = charmBonuses.Where(x => x.CharmUseId == charmUse.IdKey).ToList();

                foreach (CharmBonus bonus in currentBonuses)
                {
                    if (bonus.EntityTypeId != EntityTypes.Stat)
                    {
                        continue;
                    }

                    StatType stat = statTypes.FirstOrDefault(x => x.IdKey == bonus.EntityId);

                    if (stat == null)
                    {
                        continue;
                    }

                    int matchingBitsCount = 0;
                    long bonusQuantity = 0;

                    for (int i = 0; i < bonus.CheckBitCount; i++)
                    {
                        int index = (int)((bonus.CheckBitStartIndex + i * bonus.CheckBitSkip) % CharmConstants.CharmIdBitLength);

                        if (bits[index] == bonus.CheckBitValue)
                        {
                            matchingBitsCount++;
                        }
                    }

                    if (matchingBitsCount == bonus.CheckBitsMatchTarget)
                    {
                        bonusQuantity = 0;
                        if (bonus.QuantityBonusType == CharmBonusQuantityTypes.BinaryValue)
                        {
                            for (int b = 0; b < bonus.QuantityBitsCount; b++)
                            {
                                int index = (int)(bonus.QuantityStartBit - b * bonus.QuantityBitSkip);
                                while (index < 0)
                                {
                                    index += CharmConstants.CharmIdBitLength;
                                }
                                bonusQuantity += (1 << (bits[index % CharmConstants.CharmIdBitLength] ? 1 : 0));
                            }
                        }
                        else if (bonus.QuantityBonusType == CharmBonusQuantityTypes.BitQuantity)
                        {
                            for (int b = 0; b < bonus.QuantityBitsCount; b++)
                            {
                                int index = (int)(bonus.QuantityStartBit - b * bonus.QuantityBitSkip);
                                while (index < 0)
                                {
                                    index += CharmConstants.CharmIdBitLength;
                                }
                                bonusQuantity += bits[index % CharmConstants.CharmIdBitLength] ? 1 : 0;
                            }
                            bonusQuantity *= bonus.BonusQuantitySkip;
                            bonusQuantity += bonus.BonusQuantityStart;
                        }
                    }

                    if (bonusQuantity != 0)
                    {
                        list.Bonuses.Add(new PlayerCharmBonus()
                        {
                            EntityTypeId = bonus.EntityTypeId,
                            EntityId = bonus.EntityId,
                            Quantity = bonusQuantity,
                        });
                    }

                    list.Bonuses = list.Bonuses.OrderBy(x=>x.EntityTypeId).ThenBy(x=>x.EntityId).ToList();
                }
            }
            return retval;
        }

        public List<string> PrintBonuses(GameState gs, PlayerCharmBonusList list)
        {

            List<string> retval = new List<string>();
            List<StatType> allStatTypes = gs.data.GetGameData<StatSettings>(null).GetList<StatType>();

            foreach (PlayerCharmBonus bonus in list.Bonuses)
            {
                if (bonus.Quantity == 0 || bonus.EntityTypeId != EntityTypes.Stat)
                {
                    continue;
                }

                StatType statType = allStatTypes.FirstOrDefault(x => x.IdKey == bonus.EntityId);
                if (statType == null)
                {
                    continue;
                }

                retval.Add(statType.Name + ": +" + bonus.Quantity + "%");
            }

            return retval;
        }
    }
}
