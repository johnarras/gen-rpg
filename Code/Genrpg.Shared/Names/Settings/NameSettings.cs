using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Inventory.Settings.ItemTypes;

namespace Genrpg.Shared.Names.Settings
{
    [MessagePackObject]
    public class WeightedName
    {
        [Key(0)] public float Weight { get; set; }
        [Key(1)] public bool Ignore { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public string Desc { get; set; }

        public WeightedName()
        {
            Weight = 1000;
            Ignore = false;
            Name = "";
            Desc = "";
        }
    }
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
        [Key(2)] public override string Name { get; set; }
        [Key(3)] public string ListName { get; set; }
        [Key(4)] public List<WeightedName> Names { get; set; }

        public NameList()
        {
            Names = new List<WeightedName>();
        }
    }

    [MessagePackObject]
    public class NameSettingsApi : ParentSettingsApi<NameSettings, NameList> { }
    [MessagePackObject]
    public class NameSettingsLoader : ParentSettingsLoader<NameSettings, NameList> { }

    [MessagePackObject]
    public class ItemSettingsMapper : ParentSettingsMapper<NameSettings, NameList, NameSettingsApi> { }

}
