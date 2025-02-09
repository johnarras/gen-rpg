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
    public class SmallIdShortCollection : BaseSmallIdCollection<short>
    {
        protected override short InternalAdd(short first, short second)
        {
            return (short)(first + second);
        }

        protected override bool IsDefault(short t)
        {
            return t == 0;
        }
    }
}