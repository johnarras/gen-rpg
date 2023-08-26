using MessagePack;

using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.Users.Entities
{
    public class UserFlags
    {
        public const int ChatActive = 1 << 0;
        public const int SoundActive = 1 << 1;
        public const int MusicActive = 1 << 2;
    }

    [MessagePackObject]
    public class User : BasePlayerData, IName
    {
        /// <summary>
        /// Used for the id found in the relational database
        /// </summary>
        /// 
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long AccountId { get; set; }
        [Key(2)] public string Email { get; set; }
        [Key(3)] public int Version { get; set; }
        [Key(4)] public string Name { get; set; }
        [Key(5)] public string SessionId { get; set; }
        [Key(6)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }

        public string GetNoUnderscoreName()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return "";
            }

            return Name.Replace("_", " ");
        }

    }
}
