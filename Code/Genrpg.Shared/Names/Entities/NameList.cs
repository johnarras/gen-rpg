using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Names.Entities
{

    [MessagePackObject]
    public class NameList
    {
        [Key(0)] public string ListName { get; set; }
        [Key(1)] public List<WeightedName> Names { get; set; }

        public NameList()
        {
            Names = new List<WeightedName>();
        }


    }
}
