namespace CelticCode;

using System;
using System.Numerics;

using CelticCode.Editor;
using CelticCode.Freetype;

using RaylibSharp;

public class Application : IDisposable
{
    TextEditor editor;
    Shader shader;
    Font font;
    int x = 1000;
    int lineHeight;

    public Application()
    {
        editor = new()
        {
            OnFileContentChanged = (TextFile file) => { }
        };

        editor.NewFile();

        shader = Raylib.LoadFragmentShader("Assets/Shaders/font.frag");

        font = FontAtlas.GenerateSubpixelTexture("Assets/Fonts/CascadiaCode.ttf", 12, out lineHeight);
    }

    public void Update(double dt)
    {
        _ = dt;

        if (Raylib.IsKeyPressed(Key.Enter))
        {
            Raylib.UnloadTexture(font.Texture);
            Raylib.UnloadShader(shader);
            shader = Raylib.LoadFragmentShader("Assets/Shaders/font.frag");
            font = FontAtlas.GenerateSubpixelTexture("Assets/Fonts/CascadiaCode.ttf", 12, out lineHeight);
        }

        if (Raylib.IsKeyDown(Key.Left))
        {
            x -= 10;
        }

        if (Raylib.IsKeyDown(Key.Right))
        {
            x += 10;
        }

        Key key = (Key)Raylib.GetKeyPressed();

        while (key != Key.Null)
        {
            editor.InsertTextAtCursors((char)key);
            key = (Key)Raylib.GetKeyPressed();
        }
    }

    public void Render(double dt)
    {
        _ = dt;

        Raylib.BeginDrawing();
        {
            Raylib.ClearBackground(25, 29, 31);
            Raylib.DrawTexture(font.Texture, new Vector2(x, 0), 0, 8, Color.White);

            string[] lines = [
                "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ |",
                // "WX",
                // "W",
                // "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVW",
                // "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ"
            ];

            for (int i = 0; i < lines.Length; i++)
            {
                DrawText(lines[i], new Vector2(0, i * lineHeight), new Color(192));
            }
        }
        Raylib.EndDrawing();
    }

    void DrawText(string text, Vector2 position, Color color)
    {
        Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "fsin_background"), new Color(25, 29, 31));
        Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "fsin_foreground"), color);
        Raylib.BeginShaderMode(shader);
        {
            Raylib.DrawText(font, text, position, 12, 0, Color.White);
        }
        Raylib.EndShaderMode();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
