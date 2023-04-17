namespace CelticCode.Renderer;

using System.Numerics;

using Veldrid;

public readonly struct Vertex
{
    public static uint SizeInBytes { get; } = sizeof(float) * 10;

    public float X { get; }
    public float Y { get; }

    public float U { get; }
    public float V { get; }

    public float Br { get; }
    public float Bg { get; }
    public float Bb { get; }

    public float Fr { get; }
    public float Fg { get; }
    public float Fb { get; }

    public Vertex(float x, float y, float u, float v, float br, float bg, float bb, float fr, float fg, float fb)
    {
        X = x;
        Y = y;
        U = u;
        V = v;
        Br = br;
        Bg = bg;
        Bb = bb;
        Fr = fr;
        Fg = fg;
        Fb = fb;
    }

    public Vertex(Vector2 pos, Vector2 uv, RgbaFloat background, RgbaFloat foreground)
    {
        X = pos.X;
        Y = pos.Y;

        U = uv.X;
        V = uv.Y;

        Br = background.R;
        Bg = background.G;
        Bb = background.B;

        Fr = foreground.R;
        Fg = foreground.G;
        Fb = foreground.B;
    }
}
