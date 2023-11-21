using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.GameSettings
{
    public abstract class ChildSettings : BaseGameSettings
    {
        [MessagePack.IgnoreMember]
        public abstract string ParentId { get; set; }

        [MessagePack.IgnoreMember]
        public abstract override string Name { get; set; }
    }
}
