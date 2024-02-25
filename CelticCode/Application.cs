namespace CelticCode;

using System;
using System.Numerics;

using CelticCode.Freetype;

using RaylibSharp;

public class Application : IDisposable
{
    Shader shader;
    Font font;
    int lineHeight;

    string lines;
    (int, int) screenTextDimensions;

    public Application(string path)
    {
        lines = File.ReadAllText(path);

        shader = Raylib.LoadFragmentShader("Assets/Shaders/font.frag");
        Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "background"), new(0x191d1fFF));
        Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "foreground"), new(0xC0C0C0FF));

        font = FontAtlas.GenerateSubpixelTexture("Assets/Fonts/CascadiaCode.ttf", 12, out lineHeight, 5);
    }

    public void Update(double dt)
    {
        screenTextDimensions = ((int)MathF.Ceiling(Raylib.GetScreenWidth() / font.Recs['a'].Width), (int)MathF.Ceiling(Raylib.GetScreenHeight() / (float)lineHeight));

        _ = dt;

        Key key;

        do
        {
            key = (Key)Raylib.GetKeyPressed();
            if (key != Key.Null && key > Key.Apostrophe && key < Key.Grave)
            {
                Console.WriteLine(key);
            }
        }
        while (key != Key.Null);
    }

    public void Render(double dt)
    {
        _ = dt;

        Raylib.BeginDrawing();
        {
            Raylib.ClearBackground(25, 29, 31);

            Raylib.BeginShaderMode(shader);
            Raylib.BeginBlendMode(BlendMode.Additive);
            {
                Raylib.DrawText(font, lines, new Vector2(0, 0), 12, 0, Color.White);
            }
            Raylib.EndBlendMode();
            Raylib.EndShaderMode();
        }
        Raylib.EndDrawing();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
