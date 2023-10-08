using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
public interface IShapeService : IService
{
    List<MyPointF> CreateRegularPolygon(double cx, double cy, double radius, int sides, double radianOffset = Math.PI / 2.0);

}

public class ShapeService : IShapeService
{
    public List<MyPointF> CreateRegularPolygon(double cx, double cy, double radius, int sides, double radianOffset = Math.PI / 2.0)
    {
        List<MyPointF> list = new List<MyPointF>();

        if (radius <= 0 || sides < 1)
        {
            return list;
        }

        double rads = radianOffset;

        for (int p = 0; p <= sides; p++)
        {
            rads = radianOffset + p * Math.PI * 2.0 / sides;
            double sin = Math.Sin(rads);
            double cos = Math.Cos(rads);

            double x = cx + cos * radius;
            double y = cy + sin * radius;

            list.Add(new MyPointF((float)x, (float)y));

        }

        return list;
    }
}
