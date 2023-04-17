namespace CelticCode;

using System;

using CelticCode.Extension;
using CelticCode.Renderer;

using Silk.NET.Core;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;

public class Application : IDisposable
{
    private readonly IWindow window;

    private VeldridRenderer renderer;

    private Editor editor;

    public Application(IWindow window)
    {
        this.window = window;
    }

    public void Load()
    {
        RawImage icon = new(32, 32, Icon.Data);
        window.SetWindowIcon(ref icon);
        window.Center();

        renderer = new(window.CreateGraphicsDevice());

        editor = new();
        editor.OnFileContentChanged += renderer.FileContentChanged;
        editor.NewFile();

        renderer.Resize(window.Size);

        HandleInput();
    }

    private void HandleInput()
    {
        IInputContext input = window.CreateInput();
        IKeyboard keyboard = input.Keyboards[0];
        IMouse mouse = input.Mice[0];

        mouse.Cursor.StandardCursor = StandardCursor.IBeam;

        keyboard.KeyDown += (k, key, i) =>
        {
            if (key == Key.Enter)
            {
                editor.InsertNewlineAtCursors();
            }
        };

        keyboard.KeyChar += (k, key) =>
        {
            editor.InsertTextAtCursors(key);
        };
    }

    public void Update(double dt)
    {
        _ = dt;
    }

    public void Draw(double dt)
    {
        _ = dt;

        renderer.Draw();
    }

    public void Resize(Vector2D<int> size)
    {
        renderer.Resize(size);

        window.DoUpdate();
        window.DoRender();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
