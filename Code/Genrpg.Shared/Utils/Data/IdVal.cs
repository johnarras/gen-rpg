using MessagePack;
using System.Collections.Generic;
using System.Linq;
namespace Genrpg.Shared.Utils.Data
{
    [MessagePackObject]
    public class IdVal
    {
        [Key(0)] public long Id { get; set; }
        [Key(1)] public long Val { get; set; }
    }

    [MessagePackObject]
    public class IdValList
    {
        [Key(0)] public List<IdVal> _vals { get; set; } = new List<IdVal>();

        public long Get(long id)
        {
            return _vals.FirstOrDefault(x => x.Id == id)?.Val ?? 0;
        }

        public void Add(long id, long val)
        {
            Set(id, Get(id) + val);
        }

        public void Set(long id, long val)
        {
            IdVal idVal = _vals.FirstOrDefault(x => x.Id == id);
            if (idVal == null)
            {
                idVal = new IdVal() { Id = id };
                _vals.Add(idVal);
            }
            idVal.Val = val;
        }
    }
}
