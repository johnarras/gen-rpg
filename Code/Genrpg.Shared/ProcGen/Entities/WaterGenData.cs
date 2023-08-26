using MessagePack;
namespace Genrpg.Shared.ProcGen.Entities
{

    [MessagePackObject]
    public class WaterGenData
    {

        public const int DefaultMinSize = 10;
        public const int DefaultMaxSize = 90;

        public int x;
        public int z;
        public float maxHeight;
        public int stepSize;
        public int minXSize;
        public int maxXSize;
        public int minZSize;
        public int maxZSize;

        public WaterGenData()
        {
            stepSize = 1;
            minXSize = DefaultMinSize;
            minZSize = DefaultMinSize;
            maxXSize = DefaultMaxSize;
            maxZSize = DefaultMaxSize;
        }
    }
}
