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
    private readonly List<IFigure> figures = new();
    private readonly List<Glyph> glyphs = new();

    private Figure currentFigure;

    private Image<Rgba32> fontImage;

    private FontRectangle currentBound;
    private Vector2 currentPoint;

    public GlyphRenderer()
    {
        fontImage = new Image<Rgba32>(512, 512, new Rgba32(0, 0, 0, 255));
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

        // float padding = Math.Max(bounds.Width, bounds.Height) * 0.05f;
        // currentBound =  bounds.Inflate(padding, padding);
        currentBound = bounds;

        return true;
    }

    public void BeginFigure()
    {
        currentFigure = new();
    }

    public void EndFigure()
    {
        IFigure figure = currentFigure;
        for (int j = 0; j < currentFigure.Lines.Count; j++)
        {
            for (int i = figures.Count - 1; i >= 0; i--)
            {
                Line line = currentFigure.Lines[j];
                bool inside = figures[i].Intersects(line);

                if (inside)
                {
                    CompositeFigure compositeFigure = new();
                    compositeFigure.Figures.Add((Figure)figures[i]);
                    compositeFigure.Figures.Add(currentFigure);
                    figure = compositeFigure;
                    figures.RemoveAt(i);
                }
            }
        }

        figures.Add(figure);
    }

    public void EndGlyph()
    {
        glyphs.Add(new Glyph(figures.ToArray(), currentBound));
    }

    public void EndText()
    {
        List<string> textLines = new();

        int x = 50;
        int y = 50;

        foreach (Glyph glyph in glyphs)
        {
            RenderGlyph(glyph, x, y);

            x += (int)glyph.Bounds.Width * 3;
        }

        fontImage.SaveAsPng("Font.png");

        // Add the SVG header
        textLines.Add("""<svg viewBox="0 0 400 400" xmlns="http://www.w3.org/2000/svg">""");

        foreach (Glyph glyph in glyphs)
        {
            FontRectangle bounds = glyph.Bounds;

            textLines.Add($"""<rect x="{bounds.X}" y="{bounds.Y}" width="{bounds.Width}" height="{bounds.Height}" stroke="red" fill="none" />""");

            foreach (IFigure figure in glyph.Figures)
            {
                textLines.Add($"<g>");
                foreach (Line line in figure.Lines)
                {
                    textLines.Add($"""<line x1="{line.Start.X}" y1="{line.Start.Y}" x2="{line.End.X}" y2="{line.End.Y}" stroke="{line.Color}" />""");
                }
                textLines.Add($"</g>");
            }
        }

        textLines.Add("</svg>");

        File.WriteAllLines("test.svg", textLines);
    }

    private void RenderGlyph(Glyph glyph, int offsetX, int offsetY)
    {
        for (int x = 0; x < Math.Ceiling(glyph.Bounds.Width); x++)
        {
            for (int y = 0; y < Math.Ceiling(glyph.Bounds.Height); y++)
            {
                Vector2 point = new Vector2(x, y) + glyph.Bounds.Location;

                int aa = 0;

                if (glyph.Contains(point + new Vector2(0.25f, 0.25f))) { aa += 64; }
                if (glyph.Contains(point + new Vector2(0.75f, 0.25f))) { aa += 64; }
                if (glyph.Contains(point + new Vector2(0.25f, 0.75f))) { aa += 64; }
                if (glyph.Contains(point + new Vector2(0.75f, 0.75f))) { aa += 63; }

                byte r = (byte)aa;
                byte g = (byte)aa;
                byte b = (byte)aa;
                byte a = 255;

                fontImage[x + offsetX, y + offsetY] = new Rgba32(r, g, b, a);
            }
        }
    }

    public void MoveTo(Vector2 point)
    {
        currentPoint = point;
    }

    public void LineTo(Vector2 point)
    {
        AddLine(new(currentPoint, point));

        currentPoint = point;
    }

    public void CubicBezierTo(Vector2 secondControlPoint, Vector2 thirdControlPoint, Vector2 point)
    {
        AddLine(new(currentPoint, point));

        currentPoint = point;

        throw new NotImplementedException();
    }

    public void QuadraticBezierTo(Vector2 secondControlPoint, Vector2 point)
    {
        Vector2 quater = QuadraticBezier(currentPoint, point, secondControlPoint, 0.25f);
        Vector2 half = QuadraticBezier(currentPoint, point, secondControlPoint, 0.5f);
        Vector2 threequater = QuadraticBezier(currentPoint, point, secondControlPoint, 0.75f);

        AddLine(new(currentPoint, quater));
        AddLine(new(quater, half));
        AddLine(new(half, threequater));
        AddLine(new(threequater, point));

        currentPoint = point;
    }

    private void AddLine(Line line)
    {
        currentFigure.AddLine(line);
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
