using MessagePack;
namespace Genrpg.Shared.Utils.Data
{
    [MessagePackObject]
    public class MyPoint
    {
        [Key(0)] public int X { get; set; }
        [Key(1)] public int Y { get; set; }

        public MyPoint()
        {

        }
        public MyPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }


    [MessagePackObject]
    public class PointXZ
    {
        [Key(0)] public int X { get; set; }
        [Key(1)] public int Z { get; set; }

        public PointXZ()
        {

        }
        public PointXZ(int x, int z)
        {
            X = x;
            Z = z;
        }
    }

    [MessagePackObject]
    public class MyPoint2
    {
        [Key(0)] public float X { get; set; }
        [Key(1)] public float Y { get; set; }

        public MyPoint2()
        {

        }

        public MyPoint2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public MyPoint2(MyPointF pt)
        {
            if (pt == null)
            {
                return;
            }

            X = pt.X;
            Y = pt.Y;
        }


    }

    [MessagePackObject]
    public class MyPointF
    {
        [Key(0)] public float X { get; set; }
        [Key(1)] public float Y { get; set; }
        [Key(2)] public float Z { get; set; }

        public MyPointF()
        {

        }
        public MyPointF(float x, float y, float z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public MyPointF(MyPointF pt)
        {
            if (pt == null)
            {
                return;
            }

            X = pt.X;
            Y = pt.Y;
            Z = pt.Z;
        }
    }

    [MessagePackObject]
    public class MyRect
    {
        [Key(0)] public float X { get; set; }
        [Key(1)] public float Y { get; set; }
        [Key(2)] public float Width { get; set; }
        [Key(3)] public float Height { get; set; }

        public MyRect()
        {
        }

        public MyRect(float x, float y, float w, float h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }
    }

    [MessagePackObject]
    public class MySize
    {
        [Key(0)] public float Width { get; set; }
        [Key(1)] public float Height { get; set; }

    }
}
