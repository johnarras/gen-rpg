using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.PlayerGroups.Entities
{
    [MessagePackObject]
    public class PlayerGroup
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Description { get; set; }
        [Key(3)] public int GroupType { get; set; }

        [Key(4)] public List<GroupMember> Members { get; set; }

        public PlayerGroup()
        {
            Members = new List<GroupMember>();
        }
    }
}
