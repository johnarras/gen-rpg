using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;

namespace Genrpg.Shared.Names.Entities
{
    [MessagePackObject]
    public class NameSettings : ParentSettings<NameList>
    { 
        [Key(0)] public override string Id { get; set; }



        public NameList GetNameList(string nm)
        {
            if (_data == null)
            {
                return null;
            }

            foreach (NameList nl in _data)
            {
                if (nl.ListName == nm)
                {
                    return nl;
                }
            }
            return null;
        }

    }



    [MessagePackObject]
    public class NameList : ChildSettings
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public string ListName { get; set; }
        [Key(3)] public List<WeightedName> Names { get; set; }

        public NameList()
        {
            Names = new List<WeightedName>();
        }
    }

    [MessagePackObject]
    public class NameSettingsApi : ParentSettingsApi<NameSettings, NameList> { }
    [MessagePackObject]
    public class NameSettingsLoader : ParentSettingsLoader<NameSettings,NameList,NameSettingsApi> { }

}
