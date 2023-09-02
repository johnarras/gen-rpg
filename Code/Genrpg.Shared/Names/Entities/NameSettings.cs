using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Names.Entities
{
    [MessagePackObject]
    public class NameSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<NameList> NameLists { get; set; }



        public NameList GetNameList(string nm)
        {
            if (NameLists == null)
            {
                return null;
            }

            foreach (NameList nl in NameLists)
            {
                if (nl.ListName == nm)
                {
                    return nl;
                }
            }
            return null;
        }

    }
}
