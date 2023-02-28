namespace CelticCode.FontRenderer;

using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class Figure : IFigure
{
    public List<Line> Lines { get; set; } = new();

    public void AddLine(Line line)
    {
        Lines.Add(line);
    }

    public bool Intersects(Line line)
    {
        foreach (Line l in Lines)
        {
            if (PolygonIntersection.IsIntersect(l, line))
            {
                l.Color = "Red";
                return true;
            }
        }

        return false;
    }

    public bool Contains(Vector2 point)
    {
        return PolygonIntersection.CheckInside(Lines.ToArray(), point);
    }
}

public class CompositeFigure : IFigure
{
    public List<Figure> Figures { get; set; } = new();

    public List<Line> Lines => Figures.SelectMany(f => f.Lines).ToList();

    public bool Intersects(Line line)
    {
        foreach (Figure figure in Figures)
        {
            foreach (Line l in figure.Lines)
            {
                if (PolygonIntersection.IsIntersect(l, line))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool Contains(Vector2 point)
    {
        bool inside = false;
        foreach (Figure figure in Figures)
        {
            inside |= PolygonIntersection.CheckInside(figure.Lines.ToArray(), point);
        }

        return inside;
    }
}

public interface IFigure
{
    public List<Line> Lines { get; }

    public bool Intersects(Line line);
    public bool Contains(Vector2 point);

}
