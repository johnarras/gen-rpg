using MessagePack;
using System;

namespace Genrpg.Shared.Utils.Data
{
    /// <summary>
    /// This is a small, densely-packed collection of integers to try to make savefiles smaller.
    /// Used for things like stats, currencies and tiles that should have most small integers
    /// used at most times.
    /// </summary>
    [MessagePackObject]
    public class SmallIdCollection
    {

        public int MaxSize = 64;
        [Key(0)] public int[] _dat { get; set; } = new int[4];

        public void Clear()
        {
            _dat = new int[4];
        }

        public int GetLength()
        {
            return _dat.Length;
        }


        public long Get(long id)
        {
            if (id >= _dat.Length)
            {
                return 0;
            }
            return _dat[id];
        }

        public void Set(long id, long val)
        {
            if (id >= MaxSize)
            {
                throw new Exception($"CollectionContainer is capped at size {MaxSize - 1} to keep it small.");
            }

            int length = _dat.Length;

            while (length <= id)
            {
                length *= 2;
            }

            int[] newData = new int[length];

            for (int i = 0; i < _dat.Length; i++)
            {
                newData[i] = _dat[i];
            }

            _dat = newData;

            _dat[id] = (int)val;
        }

        public void Add(long id, long val)
        {
            Set(id, Get(id) + val);
        }

        public bool Has(long id)
        {
            return Get(id) > 0;
        }
    }
}
