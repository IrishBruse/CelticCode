namespace CelticCode;

using System;
using System.IO;
using System.Text;

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;

using Veldrid;
using Veldrid.SPIRV;

public class Application : IDisposable
{
    private GraphicsDevice GraphicsDevice { get; set; }
    private ResourceFactory factory;
    private CommandList commandList;

    private static DeviceBuffer vertexBuffer;
    private static DeviceBuffer indexBuffer;
    private static Shader[] shaders;
    private static Pipeline pipeline;

    private readonly IWindow window;

    private readonly string fontTTF = "Fonts/CascadiaCode.ttf";
    private string[] file;
    private float scroll;

    private const string VertexCode = @"
    #version 450

    layout(location = 0) in vec2 Position;

    void main()
    {
        gl_Position = vec4(Position, 0, 1);
    }";

    private const string FragmentCode = @"
    #version 450

    layout(location = 0) out vec4 fsout_Color;

    void main()
    {
        fsout_Color = vec4(1.0, 0.0, 0.0, 1.0);
    }";

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

        vertexBuffer = factory.CreateBuffer(new BufferDescription(4 * 16, BufferUsage.VertexBuffer));
        indexBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));

        GraphicsDevice.UpdateBuffer(vertexBuffer, 0, new[] { -1f, 1f, 1f, 1f, -1f, -1f, 1f, -1f });
        GraphicsDevice.UpdateBuffer(indexBuffer, 0, new ushort[] { 0, 1, 2, 3 });

        VertexLayoutDescription vertexLayout = new(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
        );

        ShaderDescription vertexShaderDesc = new(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main");
        ShaderDescription fragmentShaderDesc = new(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main");
        shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

        GraphicsPipelineDescription pipelineDescription = new();
        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

        pipelineDescription.DepthStencilState = new DepthStencilStateDescription(true, true, ComparisonKind.LessEqual);
        pipelineDescription.RasterizerState = new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

        pipelineDescription.ResourceLayouts = Array.Empty<ResourceLayout>();
        pipelineDescription.ShaderSet = new ShaderSetDescription(new VertexLayoutDescription[] { vertexLayout }, shaders);

        pipelineDescription.Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription;

        pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

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
        _ = dt;

        if (counter++ > 5)
        {
            counter = 0;

            window.Title = "CelticCode";
        }
    }

    public void Draw(double dt)
    {
        _ = dt;

        commandList.Begin();
        commandList.SetFramebuffer(GraphicsDevice.MainSwapchain.Framebuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(25 / 255f, 29 / 255f, 31 / 255f, 1f));

        // Draw Quad
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
        commandList.SetPipeline(pipeline);
        commandList.DrawIndexed(4, 1, 0, 0, 0);

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

        graphicsOptions.SyncToVerticalBlank = true;

        return window.CreateGraphicsDevice(graphicsOptions);
    }
}
