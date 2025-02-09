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
    public abstract class BaseSmallIdCollection<T>
    {

        public int MaxSize = 256;
        [Key(0)] public T[] Data { get; set; } = new T[4];


        protected abstract T InternalAdd(T first, T second);
        protected abstract bool IsDefault(T t);


        public void Clear()
        {
            Data = new T[4];
        }

        public int GetLength()
        {
            return Data.Length;
        }

        public void Trim()
        {
            int maxNonzeroIndex = 0;

            for (int i = 0; i < Data.Length; i++)
            {
                if (!IsDefault(Data[i]))
                {
                    maxNonzeroIndex = i;
                }
            }

            T[] newData = new T[Math.Max(4, maxNonzeroIndex + 1)];
            for (int i = 0; i < maxNonzeroIndex + 1; i++)
            {
                newData[i] = Data[i];
            }
            Data = newData;
        }

        public T Get(long id)
        {
            if (id >= Data.Length)
            {
                return default(T);
            }
            return Data[id];
        }

        public void Set(long id, T val)
        {
            if (id >= MaxSize)
            {
                throw new Exception($"CollectionContainer is capped at size {MaxSize - 1} to keep it small.");
            }

            int length = Data.Length;

            while (length <= id)
            {
                length *= 2;
            }

            T[] newData = new T[length];

            for (int i = 0; i < Data.Length; i++)
            {
                newData[i] = Data[i];
            }

            Data = newData;

            Data[id] = val;
        }

        public void Add(long id, T val)
        {
            Set(id, InternalAdd(Get(id),val));
        }

        public bool Has(long id)
        {
            return !IsDefault(Get(id));
        }        
    }
}
