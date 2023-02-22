namespace CelticCode;

using System;

using Silk.NET.Core;
using Silk.NET.Windowing;

public class Program
{
    [STAThread]
    private static void Main()
    {
        GenerateIcon.Generate();

        WindowOptions options = WindowOptions.Default;
        options.Size = new(800, 600);
        options.ShouldSwapAutomatically = false;
        options.Title = "CelticCode";
        options.VSync = true;

        using IWindow window = Window.Create(options);
        using Application app = new(window);

        window.Load += () => Preload(window);
        window.Load += app.Load;

        window.Update += app.Update;
        window.Render += app.Draw;
        window.FramebufferResize += app.Resize;

        window.Run();
    }

    private static void Preload(IWindow window)
    {
        RawImage icon = new(32, 32, Icon.Data);
        window.SetWindowIcon(ref icon);
        window.Center();
    }
}
