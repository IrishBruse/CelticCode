namespace CelticCode;

using System;
using System.Drawing;
using System.Numerics;

using CelticCode.Editor;
using CelticCode.Freetype;

using RaylibSharp;

public class Application : IDisposable
{
    private TextEditor editor;
    private Shader shader;
    private Font font;
    private int x;

    public Application()
    {
        editor = new()
        {
            OnFileContentChanged = Console.WriteLine
        };

        editor.NewFile();

        shader = Raylib.LoadFragmentShader("Assets/Shaders/font.frag");
        Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "fsin_background"), new Vector3(25, 29, 31) / 255f);
        Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "fsin_foreground"), new Vector3(192, 192, 192) / 255f);

        font = FontAtlas.GenerateSubpixelTexture("Assets/Fonts/CascadiaCode.ttf", 12);
    }

    public void Update(double dt)
    {
        _ = dt;

        if (Raylib.IsKeyPressed(Key.Enter))
        {
            Raylib.UnloadTexture(font.Texture);
            font = FontAtlas.GenerateSubpixelTexture("Assets/Fonts/CascadiaCode.ttf", 12);
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
            Raylib.ClearBackground(Color.FromArgb(25, 29, 31));
            Raylib.DrawTexture(font.Texture, new Vector2(x, 0), 0, 8, Color.White);

            Raylib.BeginShaderMode(shader);
            {
                Raylib.DrawText(font, "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ |\nabcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ\n\nabcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ", new Vector2(0, 50), 12, 0, Color.White);
            }
            Raylib.EndShaderMode();
        }
        Raylib.EndDrawing();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
