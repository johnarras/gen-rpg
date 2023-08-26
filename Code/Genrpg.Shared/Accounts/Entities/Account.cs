using MessagePack;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.Accounts.Entities
{
    [MessagePackObject]
    public class Account : IDbId
    {
        [Key(0)] public long Id { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Email { get; set; }
        [Key(3)] public string Password { get; set; }
        [Key(4)] public string PasswordSalt { get; set; }
        [Key(5)] public DateTime CreatedOn { get; set; }

        [Key(6)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }
        public Account()
        {
            CreatedOn = DateTime.UtcNow;
        }
    }

}
