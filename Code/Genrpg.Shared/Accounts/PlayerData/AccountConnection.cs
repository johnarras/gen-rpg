using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Accounts.PlayerData
{
    [MessagePackObject]
    public class AccountConnection : BaseAccountData
    {
        [Key(0)] public override string Id { get; set; }
        /// <summary>
        /// Your account id.
        /// </summary>
        [Key(1)] public string AccountId { get; set; }
        /// <summary>
        /// The account Id of this connection (not necessarily your referrer)
        /// </summary>
        [Key(2)] public string ReferrerId { get; set; }
        /// <summary>
        /// How far from you this connection is in the graph.
        /// </summary>
        [Key(3)] public int Depth { get; set; }
        /// <summary>
        /// Which product graph this is a part of (1 = main account graph)
        /// </summary>
        [Key(4)] public long ProductId { get; set; }
        /// <summary>
        /// Which index this connection is 1 (for simple tree, vs others where we 
        /// attempt overlap.
        /// </summary>
        [Key(5)] public int Index { get; set; }
    }
}
