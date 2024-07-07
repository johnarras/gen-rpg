using MessagePack;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Utils.Data
{

    [MessagePackObject]
    public class BitList
    {
        const int BitsPerItem = 32;
        [Key(0)] public List<int[]> Dat { get; set; } = new List<int[]>();

        public bool HasBit(long index)
        {
            int subIndex = (int)(index / BitsPerItem);

            int bitRemainder = (int)(index % BitsPerItem);

            int[] item = Dat.FirstOrDefault(x => x[0] == subIndex);

            if (item != null)
            {
                return (item[1] & (1 << bitRemainder)) != 0;
            }

            return false;
        }

        public void Clear()
        {
            Dat = new List<int[]>();
        }

        public void SetBit(long index)
        {
            int subIndex = (int)(index / BitsPerItem);

            int bitRemainder = (int)(index % BitsPerItem);

            int[] item = Dat.FirstOrDefault(x => x[0] == subIndex);

            if (item == null)
            {
                item = new int[2];
                item[0] = subIndex;
                Dat.Add(item);
            }

            item[1] |= (1 << bitRemainder);
        }

        public void RemoveBit(long index)
        {
            int subIndex = (int)(index / BitsPerItem);

            int bitRemainder = (int)(index % BitsPerItem);

            int[] item = Dat.FirstOrDefault(x => x[0] == subIndex);

            if (item ==null)
            {
                return;
            }

            item[1] &= ~(1 << bitRemainder);   
        }

        public bool MatchAnyBits(int bits)
        {
            int[] item = Dat.FirstOrDefault(x => x[0] == 0);
            if (item != null)
            {
                return (item[1] & bits) != 0;   
            }
            return false;
        }
    }
}
