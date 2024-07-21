using MessagePack;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.Accounts.PlayerData
{

    [MessagePackObject]
    public class Account : BaseAccountData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string ShareId { get; set; }
        [Key(2)] public string LowerShareId { get; set; }
        [Key(3)] public string ReferrerAccountId { get; set; }
        [Key(4)] public string Name { get; set; }
        [Key(5)] public string LowerName { get; set; }
        [Key(6)] public string Email { get; set; }
        [Key(7)] public string LowerEmail { get; set; }
        [Key(8)] public string PasswordSalt { get; set; }
        [Key(9)] public string PasswordHash { get; set; }
        [Key(10)] public DateTime CreatedOn { get; set; }
        [Key(11)] public long OriginalAccountProductId { get; set; }
        [Key(12)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }
        public Account()
        {
            CreatedOn = DateTime.UtcNow;
        }
    }

}
