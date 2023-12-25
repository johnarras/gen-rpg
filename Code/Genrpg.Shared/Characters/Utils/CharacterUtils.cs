using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Characters.PlayerData;

namespace Genrpg.Shared.Characters.Utils
{
    public static class CharacterUtils
    {
        public static void CopyDataFromTo(ICoreCharacter from, ICoreCharacter to)
        {
            to.Id = from.Id;
            to.Name = from.Name;
            to.Version = from.Version;
            to.UserId = from.UserId;
            to.CreationDate = from.CreationDate;
            to.Level = from.Level;
            to.PrevZoneId = from.PrevZoneId;
            to.ZoneId = from.ZoneId;
            to.EntityId = from.EntityId;
            to.EntityTypeId = from.EntityTypeId;
            to.FactionTypeId = from.FactionTypeId;
            to.Rot = from.Rot;
            to.Speed = from.Speed;
            to.X = from.X;
            to.Y = from.Y;
            to.Z = from.Z;
            to.MapId = from.MapId;
        }
    }
}
