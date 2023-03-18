namespace CelticCode;

using System.Numerics;

using Veldrid;

#pragma warning disable CA1051

public struct Vertex
{
    public float X;
    public float Y;

    public float U;
    public float V;

    public float Fr;
    public float Fg;
    public float Fb;

    public float Br;
    public float Bg;
    public float Bb;

    public static readonly int SizeInBytes = (2 + 2 + 3 + 3) * sizeof(float);

    public Vertex(float x, float y, float u, float v, RgbaFloat f, RgbaFloat b)
    {
        X = x;
        Y = y;

        U = u;
        V = v;

        Fr = f.R;
        Fg = f.G;
        Fb = f.B;

        Br = b.R;
        Bg = b.G;
        Bb = b.B;
    }

    public Vertex(Vector2 pos, Vector2 uv, RgbaFloat f, RgbaFloat b)
    {
        X = pos.X;
        Y = pos.Y;

        U = uv.X;
        V = uv.Y;

        Fr = f.R;
        Fg = f.G;
        Fb = f.B;

        Br = b.R;
        Bg = b.G;
        Bb = b.B;
    }
}

#pragma warning restore CA1051
