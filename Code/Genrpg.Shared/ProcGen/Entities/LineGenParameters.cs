using MessagePack;
namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class LineGenParameters
    {
        [Key(0)] public int WidthSize { get; set; }
        [Key(1)] public int MinWidthSize { get; set; }
        [Key(2)] public int MaxWidthSize { get; set; }
        [Key(3)] public int WidthSizeChangeAmount { get; set; }
        [Key(4)] public float WidthSizeChangeChance { get; set; }

        [Key(5)] public float WidthPosShiftChance { get; set; }
        [Key(6)] public int WidthPosShiftSize { get; set; }

        [Key(7)] public int InitialNoPosShiftLength { get; set; }

        [Key(8)] public float MaxWidthPosDrift { get; set; }

        [Key(9)] public long Seed { get; set; }

        [Key(10)] public int XRadius { get; set; }

        [Key(11)] public int YRadius { get; set; }


        [Key(12)] public float LinePathNoiseScale { get; set; }

        [Key(13)] public int MinOverlap { get; set; }

        [Key(14)] public bool UseOvalWidth { get; set; }

        [Key(15)] public int XMin { get; set; }
        [Key(16)] public int YMin { get; set; }

        [Key(17)] public int XMax { get; set; }
        [Key(18)] public int YMax { get; set; }

        public LineGenParameters()
        {
            WidthSize = 1;
            MaxWidthSize = 1;
            MinWidthSize = 1;
            WidthSizeChangeAmount = 0;
            WidthSizeChangeChance = 0f;



            WidthPosShiftChance = 0f;
            WidthPosShiftSize = 0;
            InitialNoPosShiftLength = 0;
            LinePathNoiseScale = 0.0f;
            Seed = 0;
            MinOverlap = 1;


            XMin = -1000;
            YMin = -1000;
            XMax = 100000;
            YMax = 100000;

        }
    }
}
