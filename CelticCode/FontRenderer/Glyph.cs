namespace CelticCode.FontRenderer;

using System.Linq;
using System.Numerics;

using SixLabors.Fonts;

public record Glyph(IFigure[] Figures, FontRectangle Bounds)
{
    public bool Contains(Vector2 point)
    {
        return Figures.Count(f => f.Contains(point)) % 2 == 1;
    }
}
