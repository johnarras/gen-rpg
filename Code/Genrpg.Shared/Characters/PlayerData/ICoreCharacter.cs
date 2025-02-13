﻿using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Characters.PlayerData
{
    public interface ICoreCharacter : IFilteredObject
    {
        string Name { get; set; }
        string UserId { get; set; }
        string MapId { get; set; }
        int Version { get; set; }
        float X { get; set; }
        float Y { get; set; }
        float Z { get; set; }
        float Rot { get; set; }
        float Speed { get; set; }
        long ZoneId { get; set; }
        long FactionTypeId { get; set; }
        long EntityTypeId { get; set; }
        long EntityId { get; set; }
        long SexTypeId { get; set; }
        
    }

}
