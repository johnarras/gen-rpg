using MessagePack;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Utils.Data
{

    [MessagePackObject]
    public class BitList
    {

        [Key(0)] public List<BitListItem> Data { get; set; }

        public BitList()
        {
            Data = new List<BitListItem>();
        }

        public bool HasBit(long index)
        {
            BitListItem bitem = GetBitListItem(index);
            if (bitem != null)
            {
                return bitem.HasBit(index);
            }

            return false;
        }

        public void SetBit(long index)
        {
            BitListItem bitem = GetBitListItem(index, true);
            if (bitem == null)
            {
                return;
            }

            bitem.SetBit(index);
        }

        public void RemoveBit(long index)
        {
            BitListItem bitem = GetBitListItem(index);
            if (bitem == null)
            {
                return;
            }

            bitem.RemoveBit(index);
        }

        protected BitListItem GetBitListItem(long index, bool createIfNotExist = false)
        {
            if (index < 0)
            {
                return null;
            }

            if (Data == null)
            {
                Data = new List<BitListItem>();
            }
            int desiredIndex = (int)(index / BitListItem.BitsPerItem);

            BitListItem currItem = Data.FirstOrDefault(x => x.StartIndex == desiredIndex);

            if (currItem == null && createIfNotExist)
            {
                currItem = new BitListItem() { StartIndex = desiredIndex };
                Data.Add(currItem);
            }
            return currItem;
        }
    }
}
