using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{

    public interface IChildSettings
    {

    }

    public abstract class ChildSettings : BaseGameSettings, IChildSettings
    {
        [MessagePack.IgnoreMember]
        public abstract string ParentId { get; set; }

        [MessagePack.IgnoreMember]
        public abstract override string Name { get; set; }

        public override void AddTo(GameData data)
        {
        }
    }
}
