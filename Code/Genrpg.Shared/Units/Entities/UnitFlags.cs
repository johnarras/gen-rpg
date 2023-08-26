using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Units.Entities
{
    [MessagePackObject]
    public class UnitFlags
    {
        public const int IsDead = 1 << 0;
        public const int ProcessedDeath = 1 << 1;
        public const int DidLoot = 1 << 2;
        public const int CanSkillLoot = 1 << 3;
        public const int DidSkillLoot = 1 << 4;
        public const int Evading = 1 << 5;
        public const int DidStartCombat = 1 << 6;
        public const int SuppressStatUpdates = 1 << 7;
        public const int ProxyCharacter = 1 << 8;
    }

}
