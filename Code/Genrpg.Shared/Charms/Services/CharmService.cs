using Genrpg.Shared.Charms.Constants;
using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.Charms.Settings;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Charms.Services
{
    public class CharmService : ICharmService
    {

        private IGameData _gameData = null;
        public async Task Initialize(IGameState gs, CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        public List<PlayerCharmBonusList> CalcBonuses(string charmId)
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

            IReadOnlyList<CharmUse> charmUses = _gameData.Get<CharmUseSettings>(null).GetData();

            IReadOnlyList<CharmBonus> charmBonuses = _gameData.Get<CharmBonusSettings>(null).GetData();

            IReadOnlyList<StatType> statTypes = _gameData.Get<StatSettings>(null).GetData();

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

        public List<string> PrintBonuses(PlayerCharmBonusList list)
        {

            List<string> retval = new List<string>();
            IReadOnlyList<StatType> allStatTypes = _gameData.Get<StatSettings>(null).GetData();

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
