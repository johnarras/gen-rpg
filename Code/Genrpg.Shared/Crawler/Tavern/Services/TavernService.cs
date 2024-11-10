using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Crawler.Tavern.Services
{
    public interface ITavernService : IInjectable
    {
        string GetRumor(PartyData party, CrawlerWorld world);
    }

    public class TavernService : ITavernService
    {
        private IClientRandom _rand = null;

        public string GetRumor(PartyData partyData, CrawlerWorld world)
        {
            if (world.QuestItems.Count < 1)
            {
                return "Lots of scary monsters out there.";
            }

            WorldQuestItem questItem = world.QuestItems[_rand.Next(world.QuestItems.Count)];

            if (_rand.NextDouble() < 0.35f)
            {
                List<CrawlerMap> cityMaps = world.Maps.Where(x => x.CrawlerMapTypeId == CrawlerMapTypes.City).ToList();
                List<CrawlerMap> outdoorMaps = world.Maps.Where(x => x.CrawlerMapTypeId == CrawlerMapTypes.Outdoors).ToList();

                List<CrawlerMap> okMaps = new List<CrawlerMap>();
                if (_rand.NextDouble() < 0.5f)
                {
                    okMaps = cityMaps;
                }
                else
                {
                    okMaps = outdoorMaps;
                }

                if (okMaps.Count > 0)
                {
                    CrawlerMap map = okMaps[_rand.Next(okMaps.Count)];
                    List<MapCellDetail> allExits = map.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

                    if (allExits.Count > 0)
                    {
                        MapCellDetail detail = allExits[_rand.Next(allExits.Count)];

                        CrawlerMap targetMap = world.GetMap(detail.EntityId);

                        if (targetMap != null)
                        {
                            long regionId = map.Get(detail.X,detail.Z, CellIndex.Region);

                            ZoneRegion region = map.Regions?.FirstOrDefault(x => x.ZoneTypeId == regionId) ?? null;

                            if (region != null)
                            {
                                return targetMap.Name + "\ncan be found within\n" + region.Name;
                            }
                            else
                            {
                                return targetMap.Name + "\ncan be found within\n" + map.Name;
                            }
                        }
                    }
                }
            }


            if (_rand.NextDouble() < 050f)
            {
                CrawlerMap map = world.GetMap(questItem.FoundInMapId);
                if (map == null)
                {
                    return questItem.Name + "\nis out there someplace...";
                }
                return questItem.Name + "\ncan be found in\n" + map.Name;
            }
            else 
            {
                CrawlerMap map = world.GetMap(questItem.UnlocksMapId);

                if (map == null)
                {
                    return questItem.Name + "\nunlocks a great adventure!";
                }
                return "They say that\n" + questItem.Name + "\nis needed to enter\n" + map.Name;
            }
        }
    }
}
