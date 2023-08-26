using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
namespace Genrpg.Shared.Utils.Data
{
    // This class is used to store a list of stubs of some kind and then have spaces to load the full objects referenced by
    // those stubs.

    public abstract class StubList<S, T>
        where S : IStringId
        where T : IStringId
    {
        [MessagePack.IgnoreMember] public abstract string CurrId { get; set; }
        [MessagePack.IgnoreMember] public abstract List<S> Stubs { get; set; }


        public StubList()
        {
            Stubs = new List<S>();
        }

        public int GetCount()
        {
            if (Stubs == null)
            {
                Stubs = new List<S>();
            }

            return Stubs.Count;
        }

        public S GetStub(string id)
        {
            if (Stubs == null)
            {
                return default;
            }

            foreach (S st in Stubs)
            {
                if (st.Id == id)
                {
                    return st;
                }
            }

            return default;
        }

        public void AddStub(S s)
        {
            if (s == null)
            {
                return;
            }

            if (Stubs == null)
            {
                Stubs = new List<S>();
            }

            S oldStub = default;
            foreach (S st in Stubs)
            {
                if (st.Id == s.Id) { oldStub = st; break; }
            }

            int index = -1;
            if (oldStub != null)
            {
                index = Stubs.IndexOf(oldStub);
                Stubs.Remove(oldStub);
            }
            if (index >= 0)
            {
                Stubs.Insert(index, s);
            }
            else
            {
                Stubs.Add(s);
            }

            if (string.IsNullOrEmpty(CurrId))
            {
                CurrId = s.Id;
            }
        }

        public void RemoveStub(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            if (Stubs == null)
            {
                return;
            }

            Stubs = Stubs.Where(x => x.Id != id).ToList();
            if (id == CurrId)
            {
                CurrId = "";
            }
        }
    }
}
