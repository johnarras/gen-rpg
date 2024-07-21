using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Accounts.PlayerData
{
    [MessagePackObject]
    public class ConnectionCount : BaseAccountData
    {
        [Key(0)] public override string Id { get; set; }
        /// <summary>
        /// Account Id for this user
        /// </summary>
        [Key(1)] public string AccountId { get; set; }
        /// <summary>
        /// Which product this graph is for
        /// </summary>
        [Key(2)] public long ProductId { get; set; }
        /// <summary>
        /// Which graph index this is for
        /// </summary>
        [Key(3)] public int Index { get; set; }
        /// <summary>
        /// Direct (Depth=1) connection count
        /// </summary>
        [Key(4)] public long DirectCount { get; set; }
        /// <summary>
        /// Viral (depth > 1) connection count
        /// </summary>
        [Key(5)] public long ViralCount { get; set; }

    }
}
