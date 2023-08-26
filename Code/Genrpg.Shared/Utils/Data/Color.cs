using MessagePack;
namespace Genrpg.Shared.Utils.Data
{
    /// <summary>
    ///  Color class for 0-1 float values
    ///  It is annoying using this in the editor since the editor uses
    ///  0-255 colors, but Unity uses 0.0-1.0 colors for its default color
    ///  class so it's better (IMO) to match the runtime setup for the game
    ///  rather than the extra handholding needed in the editor
    /// </summary>
    [MessagePackObject]
    public class MyColorF
    {
        [Key(0)] public float R { get; set; }
        [Key(1)] public float G { get; set; }
        [Key(2)] public float B { get; set; }
        [Key(3)] public float A { get; set; }

        public MyColorF()
        {
            R = 0;
            G = 0;
            B = 0;
            A = 0;
        }

        // Some simple operator overloads

        public static MyColorF operator *(MyColorF c1, MyColorF c2)
        {
            if (c1 == null)
            {
                return c2;
            }

            if (c2 == null)
            {
                return c1;
            }

            return new MyColorF() { R = c1.R * c2.R, G = c1.G * c2.G, B = c1.B * c2.B, A = c1.A * c2.A };
        }
        public static MyColorF operator +(MyColorF c1, MyColorF c2)
        {
            if (c1 == null)
            {
                return c2;
            }

            if (c2 == null)
            {
                return c1;
            }

            return new MyColorF() { R = c1.R + c2.R, G = c1.G + c2.G, B = c1.B + c2.B, A = c1.A + c2.A };
        }
        public static MyColorF operator -(MyColorF c1, MyColorF c2)
        {
            if (c1 == null)
            {
                return c2;
            }

            if (c2 == null)
            {
                return c1;
            }

            return new MyColorF() { R = c1.R - c2.R, G = c1.G - c2.G, B = c1.B - c2.B, A = c1.A - c2.A };
        }
        public static MyColorF operator *(MyColorF c, float val)
        {
            if (c == null)
            {
                return new MyColorF() { R = val, G = val, B = val, A = val };
            }

            return new MyColorF() { R = c.R * val, G = c.G * val, B = c.B * val, A = c.A * val };
        }



    }
}
