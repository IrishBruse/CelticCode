namespace CelticCode;

using System;

using Silk.NET.Windowing;

public class Program
{
    [STAThread]
    private static void Main()
    {
#if DEBUG
        GenerateIcon.Generate();
#endif

        WindowOptions options = WindowOptions.Default;
        options.Size = new(800, 600);
        options.ShouldSwapAutomatically = false;
        options.Title = "CelticCode";
        options.VSync = true;

        options.IsEventDriven = true;

        using IWindow window = Window.Create(options);
        using Application app = new(window);

        window.Load += app.Load;

        window.Update += app.Update;
        window.Render += app.Draw;
        window.FramebufferResize += app.Resize;

        window.Run();
    }
}
