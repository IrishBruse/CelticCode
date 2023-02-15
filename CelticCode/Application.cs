namespace CelticCode;

using System;
using System.IO;

using SharpText.Core;
using SharpText.Veldrid;

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;

using Veldrid;

public class Application : IDisposable
{
    private string[] file;

    private GraphicsDevice GraphicsDevice { get; set; }
    private ResourceFactory factory;
    private CommandList commandList;

    private Font font;
    private readonly string fontTTF = "Fonts/CascadiaCode.ttf";

    private ITextRenderer textRenderer;

    private int currentColorIndex;
    private readonly Color color = new RgbaFloat(187 / 255f, 187 / 255f, 187 / 255f, 1f).ToSharpTextColor();
    private readonly GraphicsBackend preferedBackend;
    private readonly IWindow window;
    private float scroll;
    private int fileSize = 1;

    public Application(GraphicsBackend preferedBackend, IWindow window)
    {
        this.preferedBackend = preferedBackend;
        this.window = window;
    }

    public void Load()
    {
        file = File.ReadAllLines(@"A:\CelticCode\.editorconfig");

        GraphicsDevice = CreateGraphicsDevice();
        factory = GraphicsDevice.ResourceFactory;
        commandList = factory.CreateCommandList(); font = new Font(fontTTF, 22);
        textRenderer = new VeldridTextRenderer(GraphicsDevice, commandList, font);

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

            Console.WriteLine(scroll);

            scroll = Math.Clamp(scroll, 0, fileSize - 1);
        };
    }

    public void Update(double dt)
    {
        window.Title = "CelticCode - " + Math.Round(1f / dt, 2);
        textRenderer.Update();

        int i = 0;
        foreach (string line in file)
        {
            textRenderer.DrawText(("" + i).PadLeft(4, ' ') + " " + line, new(4, (i - scroll) * 22), color);
            i++;

            if (i > fileSize)
            {
                fileSize = i;
            }
        }
    }

    public void Draw(double dt)
    {
        _ = dt;

        commandList.Begin();
        commandList.SetFramebuffer(GraphicsDevice.MainSwapchain.Framebuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(25 / 255f, 29 / 255f, 31 / 255f, 1f));

        textRenderer.Draw();

        commandList.End();
        GraphicsDevice.SubmitCommands(commandList);
        GraphicsDevice.WaitForIdle();

        GraphicsDevice.SwapBuffers();
    }

    public void Resize(Vector2D<int> size)
    {
        GraphicsDevice.MainSwapchain.Resize((uint)size.X, (uint)size.Y);
        textRenderer.ResizeToSwapchain();

        window.DoUpdate();
        window.DoRender();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        GraphicsDevice.WaitForIdle();
        textRenderer.Dispose();
        commandList.Dispose();
    }

    private GraphicsDevice CreateGraphicsDevice()
    {
        GraphicsDeviceOptions graphicsOptions = new();
        graphicsOptions.PreferStandardClipSpaceYDirection = true;
        graphicsOptions.PreferDepthRangeZeroToOne = true;
        graphicsOptions.SyncToVerticalBlank = true;
        return window.CreateGraphicsDevice(graphicsOptions, preferedBackend);
    }

    private void UpdateFont()
    {
        font = new Font(fontTTF, 20);
        textRenderer.UpdateFont(font);
    }
}
