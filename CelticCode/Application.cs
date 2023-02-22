namespace CelticCode;

using System;
using System.Drawing;
using System.IO;

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;

using Veldrid;

public class Application : IDisposable
{
    private GraphicsDevice GraphicsDevice { get; set; }
    private ResourceFactory factory;
    private CommandList commandList;

    private readonly string fontTTF = "Fonts/CascadiaCode.ttf";

    private readonly Color textColor = Color.FromArgb(255, 187, 187, 187);
    private readonly IWindow window;

    private string[] file;
    private float scroll;

    public Application(IWindow window)
    {
        this.window = window;
    }

    public void Load()
    {
        file = File.ReadAllLines(@"A:\CelticCode\.editorconfig");

        GraphicsDevice = CreateGraphicsDevice();
        factory = GraphicsDevice.ResourceFactory;
        commandList = factory.CreateCommandList();

        HandleInput();
    }

    private void HandleInput()
    {
        IInputContext input = window.CreateInput();
        IKeyboard keyboard = input.Keyboards[0];
        IMouse mouse = input.Mice[0];

        mouse.Scroll += (s, e) =>
        {
            scroll -= s.ScrollWheels[0].Y * 3;

            scroll = Math.Clamp(scroll, 0, file.Length - 1);
        };
    }

    private int counter;

    public void Update(double dt)
    {
        counter++;
        if (counter > 5)
        {
            window.Title = "CelticCode - " + Math.Round(1.0 / dt, 0) + " FPS";
            counter = 0;
        }
    }

    public void Draw(double dt)
    {
        _ = dt;

        commandList.Begin();
        commandList.SetFramebuffer(GraphicsDevice.MainSwapchain.Framebuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(25 / 255f, 29 / 255f, 31 / 255f, 1f));

        commandList.End();
        GraphicsDevice.SubmitCommands(commandList);
        GraphicsDevice.WaitForIdle();

        GraphicsDevice.SwapBuffers();
    }

    public void Resize(Vector2D<int> size)
    {
        GraphicsDevice.MainSwapchain.Resize((uint)size.X, (uint)size.Y);

        window.DoUpdate();
        window.DoRender();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        GraphicsDevice.WaitForIdle();
        commandList.Dispose();
    }

    private GraphicsDevice CreateGraphicsDevice()
    {
        GraphicsDeviceOptions graphicsOptions = new();
        graphicsOptions.PreferStandardClipSpaceYDirection = true;
        graphicsOptions.PreferDepthRangeZeroToOne = true;
        graphicsOptions.SyncToVerticalBlank = true;
        return window.CreateGraphicsDevice(graphicsOptions);
    }
}
