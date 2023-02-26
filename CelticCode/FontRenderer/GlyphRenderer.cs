namespace CelticCode.FontRenderer;

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public class GlyphRenderer : IGlyphRenderer, IDisposable
{
    private readonly List<Line> lines = new();
    private readonly List<Figure> figures = new();
    private readonly List<Glyph> glyphs = new();

    private Image<Rgba32> fontImage;

    private FontRectangle currentBound;
    private Vector2 currentPoint;

    public GlyphRenderer()
    {
        fontImage = new Image<Rgba32>(512, 512, new Rgba32(0, 0, 0, 0));
    }

    public void BeginText(FontRectangle bounds)
    {
    }

    public bool BeginGlyph(FontRectangle bounds, GlyphRendererParameters parameters)
    {
        if (parameters.GlyphIndex == 3125)
        {
            return false;
        }

        float padding = Math.Max(bounds.Width, bounds.Height) * 0.05f;
        currentBound = bounds.Inflate(padding, padding);

        return true;
    }

    public void BeginFigure()
    {
        lines.Clear();
    }

    public void EndFigure()
    {
        figures.Add(new Figure(lines.ToArray()));
    }

    public void EndGlyph()
    {
        glyphs.Add(new Glyph(figures.ToArray(), currentBound));
    }

    public void EndText()
    {
        List<string> textLines = new();
        _ = 0;

        // Add the SVG header
        textLines.Add("""<svg viewBox="0 0 2000 2000" xmlns="http://www.w3.org/2000/svg">""");

        foreach (Glyph glyph in glyphs)
        {
            textLines.Add($"<g>");

            FontRectangle bounds = glyph.Bounds;
            textLines.Add($"""<rect x="{bounds.X}" y="{bounds.Y}" width="{bounds.Width}" height="{bounds.Height}" stroke="red" fill="none" />""");

            foreach (Figure figure in glyph.Figures)
            {
                textLines.Add($"<g>");
                foreach (Line line in figure.Lines)
                {
                    textLines.Add($"""<line x1="{line.Start.X}" y1="{line.Start.Y}" x2="{line.End.X}" y2="{line.End.Y}" stroke="black" />""");
                }
                textLines.Add($"</g>");
            }

            textLines.Add($"</g>");
        }

        textLines.Add("</svg>");

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
        lines.Add(new Line(currentPoint, point));

        // lines.Add(new Line(currentPoint, QuadraticBezier(currentPoint, point, secondControlPoint, 0.25f)));
        // lines.Add(new Line(QuadraticBezier(currentPoint, point, secondControlPoint, 0.25f), QuadraticBezier(currentPoint, point, secondControlPoint, 0.5f)));
        // lines.Add(new Line(QuadraticBezier(currentPoint, point, secondControlPoint, 0.5f), QuadraticBezier(currentPoint, point, secondControlPoint, 0.75f)));
        // lines.Add(new Line(QuadraticBezier(currentPoint, point, secondControlPoint, 0.75f), point));

        currentPoint = point;
    }

    private Vector2 QuadraticBezier(Vector2 start, Vector2 end, Vector2 control, float t)
    {
        Vector2 p0 = Vector2.Lerp(start, control, t);
        Vector2 p1 = Vector2.Lerp(control, end, t);
        return Vector2.Lerp(p0, p1, t);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        fontImage.Dispose();
    }
}
