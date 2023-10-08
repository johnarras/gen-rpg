using System;
using Vec3 = UnityEngine.Vector3;

public struct GVector3
{
    public float x;
    public float y;
    public float z;

    public float magnitude
    {
        get
        {
            return (float)(Math.Sqrt(x * x + y * y + z * z));
        }
    }

    public GVector3(float xx, float yy, float zz)
    {
        x = xx;
        y = yy;
        z = zz;
    }

    public static Vec3 Create(float x, float y, float z)
    {
        return new Vec3(x, y, z);
    }
    

    public static Vec3 Create(GVector3 vec)
    {
        return new Vec3(vec.x, vec.y, vec.z);
    }

    public static GVector3 Create(Vec3 platformVector)
    {
        return new GVector3(platformVector.x, platformVector.y, platformVector.z);
    }

    public static float Distance(GVector3 a, GVector3 b)
    {
        return Vec3.Distance(GVector3.Create(a), GVector3.Create(b));
    }

    public static float Distance(Vec3 a, Vec3 b)
    {
        return Vec3.Distance(a, b);
    }

    public Vec3 ToPlatform()
    {
        return new Vec3(x, y, z);
    }


    private static GVector3 _one = new GVector3(1, 1, 1);
    public static GVector3 one { get { return _one; } }
    private static GVector3 _zero = new GVector3(0, 0, 0);
    public static GVector3 zero { get { return _zero; } }
    private static GVector3 _up = new GVector3(0, 1, 0);
    public static GVector3 up { get { return _up; } }
    private static GVector3 _down = new GVector3(0, -1, 0);
    public static GVector3 down { get { return _down; } }
    private static GVector3 _left = new GVector3(-1, 0, 0);
    public static GVector3 left { get { return _left; } }
    private static GVector3 _right = new GVector3(1, 0, 0);
    public static GVector3 right { get { return _right; } }
    private static GVector3 _forward = new GVector3(0, 0, 1);
    public static GVector3 forward { get { return _forward; } }
    private static GVector3 _back = new GVector3(0, 0, -1);
    public static GVector3 back { get { return _back; } }

    public static Vec3 onePlatform { get { return Vec3.one; } }
    public static Vec3 zeroPlatform { get { return Vec3.zero; } }
    public static Vec3 upPlatform { get { return Vec3.up; } }
    public static Vec3 downPlatform { get { return Vec3.down; } }
    public static Vec3 leftPlatform { get { return Vec3.left; } }
    public static Vec3 rightPlatform { get { return Vec3.right; } }
    public static Vec3 forwardPlatform { get { return Vec3.forward; } }
    public static Vec3 backPlatform { get { return Vec3.back; } }



    public static GVector3 operator + (GVector3 a, GVector3 b)
    {
        return new GVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static GVector3 operator -(GVector3 a, GVector3 b)
    {
        return new GVector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }


    public static GVector3 operator *(GVector3 a, float f)
    {
        return new GVector3(a.x * f, a.y * f, a.z * f);
    }
    public static GVector3 operator *(float f, GVector3 a)
    {
        return new GVector3(a.x * f, a.y * f, a.z * f);
    }

    public static GVector3 operator /(GVector3 a, float f)
    {
        if (f == 0)
        {
            throw new DivideByZeroException("GVector Divide");
        }
        return new GVector3(a.x / f, a.y / f, a.z / f);
    }

    public static GVector3 Cross (GVector3 a, GVector3 b)
    {
        return Create(Vec3.Cross(Create(a), Create(b)));
    }
}