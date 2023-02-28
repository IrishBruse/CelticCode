namespace CelticCode.FontRenderer;

using System.Numerics;

public record Line(Vector2 Start, Vector2 End)
{
    public string Color { get; set; } = "Black";

    public Line(float x1, float y1, float x2, float y2) : this(new Vector2(x1, y1), new Vector2(x2, y2))
    {

    }
}
