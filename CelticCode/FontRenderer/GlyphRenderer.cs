namespace CelticCode.FontRenderer;

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

using SixLabors.Fonts;

public class GlyphRenderer : IGlyphRenderer
{
    private readonly List<Line> lines = new();
    private readonly List<FontRectangle> rects = new();
    private Vector2 currentPoint;


    public void BeginText(FontRectangle bounds)
    {
    }

    public bool BeginGlyph(FontRectangle bounds, GlyphRendererParameters parameters)
    {
        Console.WriteLine(parameters.CodePoint + " " + parameters.GlyphIndex);
        if (parameters.GlyphIndex != 3125)
        {
            rects.Add(bounds);
        }
        // lines.Clear();
        return true;
    }

    public void BeginFigure()
    {
    }

    public void EndFigure()
    {
    }

    public void EndGlyph()
    {

    }

    public void EndText()
    {
        string start = """<svg viewBox="0 0 2000 2000" xmlns="http://www.w3.org/2000/svg">""";
        string end = """</svg>""";

        List<string> textLines = new()
        {
            start
        };

        foreach (Line line in lines)
        {
            textLines.Add($"""<line x1="{line.Start.X}" y1="{line.Start.Y}" x2="{line.End.X}" y2="{line.End.Y}" stroke="black" />""");
        }

        foreach (FontRectangle rect in rects)
        {
            textLines.Add($"""<rect x="{rect.X}" y="{rect.Y}" width="{rect.Width}" height="{rect.Height}" stroke="red" fill="none" />""");
        }
        textLines.Add(end);

        File.WriteAllLines("test.svg", textLines);
    }

    public void LineTo(Vector2 point)
    {
        lines.Add(new Line(currentPoint, point));
        currentPoint = point;
    }

    public void MoveTo(Vector2 point)
    {
        currentPoint = point;
    }

    public void CubicBezierTo(Vector2 secondControlPoint, Vector2 thirdControlPoint, Vector2 point)
    {
        lines.Add(new Line(currentPoint, point));
        currentPoint = point;
    }

    public void QuadraticBezierTo(Vector2 secondControlPoint, Vector2 point)
    {
        lines.Add(new Line(currentPoint, QuadraticBezier(currentPoint, point, secondControlPoint, 0.25f)));
        lines.Add(new Line(QuadraticBezier(currentPoint, point, secondControlPoint, 0.25f), QuadraticBezier(currentPoint, point, secondControlPoint, 0.5f)));
        lines.Add(new Line(QuadraticBezier(currentPoint, point, secondControlPoint, 0.5f), QuadraticBezier(currentPoint, point, secondControlPoint, 0.75f)));
        lines.Add(new Line(QuadraticBezier(currentPoint, point, secondControlPoint, 0.75f), point));
        currentPoint = point;
    }

    private Vector2 QuadraticBezier(Vector2 start, Vector2 end, Vector2 control, float t)
    {
        Vector2 p0 = Vector2.Lerp(start, control, t);
        Vector2 p1 = Vector2.Lerp(control, end, t);
        return Vector2.Lerp(p0, p1, t);
    }
}
