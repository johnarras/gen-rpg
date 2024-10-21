using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Tavern.Services
{
    public interface ITavernService : IInjectable
    {
        string GetRumor(PartyData party, CrawlerWorld world);
    }

    public class TavernService : ITavernService
    {
        private ICrawlerWorldService _worldService;
        private IClientRandom _rand;

        public string GetRumor(PartyData partyData, CrawlerWorld world)
        {
            if (world.QuestItems.Count < 1)
            {
                return "Lots of scary monsters out there.";
            }

            WorldQuestItem questItem = world.QuestItems[_rand.Next(world.QuestItems.Count)];

            if (_rand.NextDouble() < 0.5f)
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
