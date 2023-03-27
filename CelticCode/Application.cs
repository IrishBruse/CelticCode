namespace CelticCode;

using System;

using CelticCode.Extension;
using CelticCode.Freetype;
using CelticCode.Renderer;

using Silk.NET.Core;
using Silk.NET.Input;
using Silk.NET.Windowing;

public class Application : IDisposable
{
    private readonly IWindow window;
    private VeldridManager veldrid;
    private GeometryBuffer textBuffer;

    private Editor editor = new();

    public Application(IWindow window)
    {
        this.window = window;
    }

    public void Load()
    {
        RawImage icon = new(32, 32, Icon.Data);
        window.SetWindowIcon(ref icon);
        window.Center();

        veldrid = new(window);

        FontAtlas fontAtlas = FontAtlas.GenerateSubpixelTexture(veldrid.GraphicsDevice, "Assets/Fonts/CascadiaCode.ttf", 12);

        textBuffer = new(veldrid.GraphicsDevice);

        // TODO: Only update modified text
        editor.OnFileContentChanged += (TextFile file) =>
        {
            textBuffer.Clear();
            textBuffer.GenerateFontGeometry(file, fontAtlas);
        };

        // editor.Open("A:/CelticCode/test.txt");
        editor.NewFile();

        veldrid.Setup(fontAtlas);
        veldrid.Resize(window.Size);

        HandleInput();
    }

    private void HandleInput()
    {
        IInputContext input = window.CreateInput();
        IKeyboard keyboard = input.Keyboards[0];
        IMouse mouse = input.Mice[0];

        mouse.Cursor.StandardCursor = StandardCursor.IBeam;

        keyboard.KeyChar += (k, key) =>
        {
            editor.InsertTextAtCursors(key);
        };

        keyboard.KeyDown += (k, key, i) =>
        {
            switch (key)
            {
                case Key.PageDown:
                // scroll += 45;
                break;

                case Key.PageUp:
                // scroll -= 45;
                break;
            }
        };

        // mouse.Scroll += (s, e) =>
        // {
        //     const int lines = 3;

        //     scroll -= s.ScrollWheels[0].Y * lines;

        //     if (scroll < 0)
        //     {
        //         scroll = 0;
        //     }
        // };
    }

    public void Update(double dt)
    {
        _ = dt;
    }

    public void Draw(double dt)
    {
        _ = dt;

        textBuffer.Upload();

        veldrid.UpdateCameraPosition(new(0, 0, 0));
        veldrid.BeginDraw(25 / 255f, 29 / 255f, 31 / 255f);
        {
            veldrid.Draw(textBuffer);
        }
        veldrid.EndDraw();
    }

    public void Resize(Silk.NET.Maths.Vector2D<int> size)
    {
        veldrid.Resize(size);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
