﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Names.Services;
using Genrpg.Shared.Names.Settings;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Units.Settings;

namespace Genrpg.Shared.Units.Services
{

    public class UnitGenService : IUnitGenService
    {

        private INameGenService _nameGenService = null;
        private IGameData _gameData = null;
        protected IMapProvider _mapProvider;
        public virtual string GenerateUnitName(IRandom rand, long unitTypeId, long zoneId,
            Dictionary<string, string> args = null)
        {

            UnitType utype = _gameData.Get<UnitSettings>(null).Get(unitTypeId);
            if (utype == null)
            {
                return "Monster";
            }


            Zone zone = _mapProvider.GetMap().Get<Zone>(zoneId);
            if (zone == null)
            {
                return utype.Name;
            }

            ZoneType ztype = _gameData.Get<ZoneTypeSettings>(null).Get(zone.ZoneTypeId);
            if (ztype == null)
            {
                return utype.Name;
            }


            ZoneUnitStatus status = zone.GetUnit(unitTypeId);

            string alternateName = _nameGenService.PickWord(rand, utype.AlternateNames);
            string coreName = utype.Name;
            if (rand.Next() % 8 == 0 && !string.IsNullOrEmpty(alternateName))
            {
                coreName = alternateName;
            }

            if (status != null && !string.IsNullOrEmpty(status.Prefix))
            {
                return status.Prefix + " " + coreName;
            }

            return coreName;

        }

        public virtual string GenerateUnitPrefixName(IRandom rand, long unitTypeId, Zone zone,
            Dictionary<string, string> args = null)
        {

            UnitType utype = _gameData.Get<UnitSettings>(null).Get(unitTypeId);
            if (utype == null)
            {
                return "";
            }


            if (zone == null)
            {
                return utype.Name;
            }

            ZoneType ztype = _gameData.Get<ZoneTypeSettings>(null).Get(zone.ZoneTypeId);
            if (ztype == null)
            {
                return "";
            }

            string zonePrefix = _nameGenService.PickWord(rand, ztype.CreatureNamePrefixes);
            string doublePrefix = _nameGenService.PickWord(rand, ztype.CreatureDoubleNamePrefixes);
            string creaturePrefix = _nameGenService.PickWord(rand, utype.PrefixNames);
            string doubleSuffix = _nameGenService.PickWord(rand, utype.DoubleNameSuffixes);

            string categoryName = "";
            string colorName = "";

            if (args == null)
            {
                args = new Dictionary<string, string>();
            }

            NameList overallList = _gameData.Get<NameSettings>(null).GetNameList("CreatureOverallNames");
            string overallName = "";
            if (overallList != null)
            {
                overallName = _nameGenService.PickWord(rand, overallList.Names);
            }

            ZoneUnitStatus status = zone.GetUnit(unitTypeId);


            // How do we do this?
            // All prefixes/names except for the doublename come before colors.
            // Then colors come, then the doublename comes if it exists.

            string doubleName = _nameGenService.CombinePrefixSuffix(rand, doublePrefix, doubleSuffix, 0.14f);


            List<string> prefixNames = new List<string>();



            if (!string.IsNullOrEmpty(zonePrefix))
            {
                for (int times = 0; times < 2; times++)
                {
                    prefixNames.Add(zonePrefix);

                }
            }
            if (!string.IsNullOrEmpty(creaturePrefix))
            {
                for (int times = 0; times < 3; times++)
                {
                    prefixNames.Add(creaturePrefix);
                }
            }
            if (!string.IsNullOrEmpty(categoryName))
            {
                for (int times = 0; times < 1; times++)
                {
                    prefixNames.Add(categoryName);
                }
            }
            if (!string.IsNullOrEmpty(overallName))
            {
                for (int times = 0; times < 1; times++)
                {
                    prefixNames.Add(overallName);
                }
            }


            string prefixName = "";

            if (prefixNames.Count > 0)
            {
                prefixName = prefixNames[rand.Next() % prefixNames.Count];
            }


            bool usePrefix = false;
            bool useColor = false;
            bool useDouble = false;

            if (args.ContainsKey("Prefix") && !string.IsNullOrEmpty(args["Prefix"]))
            {
                prefixName = args["Prefix"];
                usePrefix = true;
            }
            if (args.ContainsKey("Color") && !string.IsNullOrEmpty(args["Color"]))
            {
                colorName = args["ColorName"];
                useColor = true;
            }
            if (args.ContainsKey("Double") && !string.IsNullOrEmpty(args["Double"]))
            {
                colorName = args["Double"];
                useDouble = true;
            }

            if (!useDouble && (!usePrefix || !useDouble) && !string.IsNullOrEmpty(doubleName))
            {
                if (rand.Next() % 6 == 0 || string.IsNullOrEmpty(prefixName) && string.IsNullOrEmpty(colorName))
                {
                    useDouble = true;
                }
            }
            if (!useColor && (!usePrefix || !useDouble) && !string.IsNullOrEmpty(colorName))
            {
                if (rand.Next() % 4 == 0 || string.IsNullOrEmpty(prefixName) && string.IsNullOrEmpty(doubleName))
                {
                    useColor = true;
                }
            }
            if (!usePrefix && (!useColor || !useDouble) && !string.IsNullOrEmpty(prefixName))
            {
                if (rand.Next() % 3 == 0 || string.IsNullOrEmpty(colorName) && string.IsNullOrEmpty(doubleName) ||
                    !useColor && !useDouble)
                {
                    usePrefix = true;
                }
            }

            if (string.IsNullOrEmpty(prefixName))
            {
                usePrefix = false;
            }

            if (string.IsNullOrEmpty(colorName))
            {
                useColor = false;
            }

            if (string.IsNullOrEmpty(doubleName))
            {
                useDouble = false;
            }

            if (!usePrefix && !useColor && !useDouble)
            {
                if (!string.IsNullOrEmpty(prefixName))
                {
                    usePrefix = true;
                }
                else if (!string.IsNullOrEmpty(colorName))
                {
                    useColor = true;
                }
                else if (!string.IsNullOrEmpty(doubleName))
                {
                    useDouble = true;
                }
            }

            if (usePrefix && useColor && useDouble)
            {
                if (rand.Next() % 6 != 0 && (!string.IsNullOrEmpty(prefixName) || !string.IsNullOrEmpty(colorName)))
                {
                    useDouble = false;
                }
                else if (rand.Next() % 3 != 0 && (!string.IsNullOrEmpty(prefixName) || !string.IsNullOrEmpty(doubleName)))
                {
                    useColor = false;
                }
                else if (!string.IsNullOrEmpty(colorName) || !string.IsNullOrEmpty(doubleName))
                {
                    usePrefix = false;
                }

            }

            // Now create the name, hopefully enough checks were done to keep it from looking too bad.
            string fullPrefix = "";

            if (usePrefix && !string.IsNullOrEmpty(prefixName))
            {
                fullPrefix += prefixName + " ";
            }
            if (useColor && !string.IsNullOrEmpty(colorName))
            {
                fullPrefix += colorName + " ";
            }
            if (useDouble && !string.IsNullOrEmpty(doubleName))
            {
                fullPrefix += doubleName + " ";
            }

            if (fullPrefix.Length > 0 && fullPrefix[fullPrefix.Length - 1] == ' ')
            {
                fullPrefix = fullPrefix.Substring(0, fullPrefix.Length - 1);
            }


            return fullPrefix;

        }



        /// <summary>
        /// Get a creature type for the given zone.
        /// Will eventually be based on zone state, but for now its random.
        /// 
        /// This will probably return an error/0 eventually in a lot of cases if
        /// the zone's creature population is too low.
        /// </summary>
        /// <param name="gs">GameState</param>
        /// <param name="map">Map</param>
        /// <param name="zone">Zone</param>
        /// <returns>Creature type Id or 0 if there's some error</returns>
        public virtual UnitType GetRandomUnitType(IRandom rand, Map map, Zone zone)
        {
            if (map == null || zone == null)
            {
                return null;
            }

            if (zone.Units == null || zone.Units.Count < 1)
            {
                return null;
            }

            return _gameData.Get<UnitSettings>(null).Get(RandomUtils.GetRandomElement(zone.Units, rand).UnitTypeId);

        }
    }
}
