using MessagePack;
namespace Genrpg.Shared.Utils
{
    [MessagePackObject]
    public class FlagUtils
    {
        public static bool IsSet(long val, long flag)
        {
            return (val & flag) != 0;
        }

    }
}
