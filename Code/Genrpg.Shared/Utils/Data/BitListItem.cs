using MessagePack;
namespace Genrpg.Shared.Utils.Data
{
    [MessagePackObject]
    public class BitListItem
    {
        public const int BitsPerItem = 32;

        [Key(0)] public int StartIndex { get; set; }
        [Key(1)] public int Bits { get; set; }

        public bool HasBit(long bit)
        {
            bit %= BitsPerItem;
            return (Bits & 1 << (int)bit) != 0;
        }

        public void SetBit(long bit)
        {
            bit %= BitsPerItem;
            Bits |= 1 << (int)bit;
        }

        public void RemoveBit(long bit)
        {
            bit %= BitsPerItem;
            Bits &= ~(1 << (int)bit);
        }
    }
}
