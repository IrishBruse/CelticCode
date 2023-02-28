namespace CelticCode.FontRenderer;

using System;
using System.Numerics;

public static class PolygonIntersection
{
    public static bool CheckInside(Line[] polygon, Vector2 p)
    {
        bool inside = false;

        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            Vector2 start = polygon[i].Start;
            Vector2 end = polygon[j].Start;

            bool v = (start.Y > p.Y) != (end.Y > p.Y);
            bool v1 = p.X < ((end.X - start.X) * (p.Y - start.Y) / (end.Y - start.Y)) + start.X;

            if (v && v1)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static int OnLine(Line l1, Vector2 p)
    {
        // Check whether p is on the line or not
        if (p.X <= Math.Max(l1.Start.X, l1.End.X) && p.X <= Math.Min(l1.Start.X, l1.End.X) && p.Y <= Math.Max(l1.Start.Y, l1.End.Y) && p.Y <= Math.Min(l1.Start.Y, l1.End.Y))
        {
            return 1;
        }

        return 0;
    }

    private static int Direction(Vector2 a, Vector2 b, Vector2 c)
    {
        int val = (int)(((b.Y - a.Y) * (c.X - b.X)) - ((b.X - a.X) * (c.Y - b.Y)));

        if (val == 0)
        {

            // Colinear
            return 0;
        }
        else if (val < 0)
        {

            // Anti-clockwise direction
            return 2;
        }

        // Clockwise direction
        return 1;
    }

    public static bool IsIntersect(Line lineA, Line lineB)
    {
        double x1 = lineA.Start.X, y1 = lineA.Start.Y;
        double x2 = lineA.End.X, y2 = lineA.End.Y;

        double tolerance = 0.0001;

        double x3 = lineB.Start.X, y3 = lineB.Start.Y;
        double x4 = lineB.End.X, y4 = lineB.End.Y;

        // equations of the form x=c (two vertical lines) with overlapping
        if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance && Math.Abs(x1 - x3) < tolerance)
        {
            return false;
        }

        //equations of the form y=c (two horizontal lines) with overlapping
        if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance && Math.Abs(y1 - y3) < tolerance)
        {
            return false;
        }

        //equations of the form x=c (two vertical parallel lines)
        if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance)
        {
            //return default (no intersection)
            return false;
        }

        //equations of the form y=c (two horizontal parallel lines)
        if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance)
        {
            //return default (no intersection)
            return false;
        }

        //general equation of line is y = mx + c where m is the slope
        //assume equation of line 1 as y1 = m1x1 + c1
        //=> -m1x1 + y1 = c1 ----(1)
        //assume equation of line 2 as y2 = m2x2 + c2
        //=> -m2x2 + y2 = c2 -----(2)
        //if line 1 and 2 intersect then x1=x2=x & y1=y2=y where (x,y) is the intersection point
        //so we will get below two equations
        //-m1x + y = c1 --------(3)
        //-m2x + y = c2 --------(4)

        double x, y;

        //lineA is vertical x1 = x2
        //slope will be infinity
        //so lets derive another solution
        if (Math.Abs(x1 - x2) < tolerance)
        {
            //compute slope of line 2 (m2) and c2
            double m2 = (y4 - y3) / (x4 - x3);
            double c2 = (-m2 * x3) + y3;

            //equation of vertical line is x = c
            //if line 1 and 2 intersect then x1=c1=x
            //subsitute x=x1 in (4) => -m2x1 + y = c2
            // => y = c2 + m2x1
            x = x1;
            y = c2 + (m2 * x1);
        }
        //lineB is vertical x3 = x4
        //slope will be infinity
        //so lets derive another solution
        else if (Math.Abs(x3 - x4) < tolerance)
        {
            //compute slope of line 1 (m1) and c2
            double m1 = (y2 - y1) / (x2 - x1);
            double c1 = (-m1 * x1) + y1;

            //equation of vertical line is x = c
            //if line 1 and 2 intersect then x3=c3=x
            //subsitute x=x3 in (3) => -m1x3 + y = c1
            // => y = c1 + m1x3
            x = x3;
            y = c1 + (m1 * x3);
        }
        //lineA & lineB are not vertical
        //(could be horizontal we can handle it with slope = 0)
        else
        {
            //compute slope of line 1 (m1) and c2
            double m1 = (y2 - y1) / (x2 - x1);
            double c1 = (-m1 * x1) + y1;

            //compute slope of line 2 (m2) and c2
            double m2 = (y4 - y3) / (x4 - x3);
            double c2 = (-m2 * x3) + y3;

            //solving equations (3) & (4) => x = (c1-c2)/(m2-m1)
            //plugging x value in equation (4) => y = c2 + m2 * x
            x = (c1 - c2) / (m2 - m1);
            y = c2 + (m2 * x);

            //verify by plugging intersection point (x, y)
            //in orginal equations (1) & (2) to see if they intersect
            //otherwise x,y values will not be finite and will fail this check
            if (!(Math.Abs((-m1 * x) + y - c1) < tolerance && Math.Abs((-m2 * x) + y - c2) < tolerance))
            {
                //return default (no intersection)
                return false;
            }
        }

        //x,y can intersect outside the line segment since line is infinitely long
        //so finally check if x, y is within both the line segments
        if (IsInsideLine(lineA, x, y) && IsInsideLine(lineB, x, y))
        {
            return true;
        }

        //return default (no intersection)
        return false;
    }
    private static bool IsInsideLine(Line line, double x, double y)
    {
        double x1 = line.Start.X, y1 = line.Start.Y;
        double x2 = line.End.X, y2 = line.End.Y;
        return ((x >= x1 && x <= x2) || (x >= x2 && x <= x1)) && ((y >= y1 && y <= y2) || (y >= y2 && y <= y1));
    }
}
