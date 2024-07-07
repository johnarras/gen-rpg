using MessagePack;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Genrpg.Shared.Utils.Data
{

    /// <summary>
    /// Use this when we know the index won't grow too much (we will mostly use lower order bits)
    /// </summary>
    [MessagePackObject]
    public class SmallIndexBitList
    {
        const int BitsPerItem = 32;

        [Key(0)] public int[] Bits { get; set; } = new int[1];

        public bool HasBit(long index)
        {
            int subIndex = (int)(index / BitsPerItem);

            int bitRemainder = (int)(index % BitsPerItem);

            if (Bits.Length <= subIndex)
            {
                return false;
            }

            return (Bits[subIndex] & (1 << bitRemainder)) != 0;

        }

        public void Clear()
        {
            Bits = new int[1];
        }

        public void SetBit(long index)
        {
            int subIndex = (int)(index / BitsPerItem);

            int bitRemainder = (int)(index % BitsPerItem);

            if (Bits.Length <= subIndex)
            {
                int[] newItems = new int[subIndex + 1];
                for (int i = 0; i < Bits.Length; i++)
                {
                    newItems[i] = Bits[i];
                }
                Bits = newItems;
            }

            Bits[subIndex] |= (1 << bitRemainder);  
        }

        public void RemoveBit(long index)
        {
            int subIndex = (int)(index / BitsPerItem);

            int bitRemainder = (int)(index % BitsPerItem);

            if (Bits.Length <= subIndex)
            {
                return;
            }

            Bits[subIndex] &= ~(1 << bitRemainder);
        }

        public bool MatchAnyBits(int bits)
        {
            if (Bits.Length < 1)
            {
                return false;
            }

            return (Bits[0] & bits) != 0;
        }
    }
}
