namespace CelticCode;

using System;
using System.IO;

using CelticCode.FontRenderer;

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;

using SixLabors.Fonts;

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

    private Texture surfaceTexture;
    private TextureView surfaceTextureView;
    private ResourceLayout resourceLayout;
    private readonly IWindow window;

    private string[] file;
    private float scroll;

    private ResourceLayout textureLayout;
    private ResourceSet textureSet;

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

        vertexBuffer = factory.CreateBuffer(new BufferDescription(4 * 32, BufferUsage.VertexBuffer));
        indexBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));

        float[] source = new[] {
            -0.5f, 0.5f, 0.0f, 1.0f,
            0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, -0.5f, 0.0f, 0.0f,
            0.5f, -0.5f, 1.0f, 0.0f
        };

        GraphicsDevice.UpdateBuffer(vertexBuffer, 0, source);
        GraphicsDevice.UpdateBuffer(indexBuffer, 0, new ushort[] { 0, 1, 2, 3 });

        VertexLayoutDescription vertexLayout = new(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
        );

        ShaderDescription vertexShaderDesc = new(ShaderStages.Vertex, File.ReadAllBytes("Shaders/base.vert"), "main");
        ShaderDescription fragmentShaderDesc = new(ShaderStages.Fragment, File.ReadAllBytes("Shaders/base.frag"), "main");
        shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

        GraphicsPipelineDescription pipelineDescription = new();
        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

        pipelineDescription.DepthStencilState = new DepthStencilStateDescription(true, true, ComparisonKind.LessEqual);
        pipelineDescription.RasterizerState = new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

        pipelineDescription.ShaderSet = new ShaderSetDescription(new VertexLayoutDescription[] { vertexLayout }, shaders);

        textureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        pipelineDescription.ResourceLayouts = new[]
        {
            textureLayout
        };

        resourceLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("TextureSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            )
        );

        pipelineDescription.Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription;
        pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        UpdateFramebuffer();
        HandleInput();

        FontCollection collection = new();
        FontFamily family = collection.Add("Fonts/CascadiaCode.ttf");
        Font font = family.CreateFont(120, FontStyle.Regular);

        GlyphRenderer glyphRenderer = new();
        TextRenderer renderer = new(glyphRenderer);
        renderer.RenderText("0 2", new(font));
    }

    private void UpdateFramebuffer()
    {
        if (window.Size.X == 0 || window.Size.Y == 0)
        {
            return;
        }

        surfaceTexture = factory.CreateTexture(TextureDescription.Texture2D(
            (uint)window.Size.X,
            (uint)window.Size.Y,
            1, 1,
            PixelFormat.R8_G8_B8_A8_UNorm,
            TextureUsage.Sampled | TextureUsage.RenderTarget
        ));

        surfaceTextureView = factory.CreateTextureView(surfaceTexture);

        textureSet = factory.CreateResourceSet(new ResourceSetDescription(
            resourceLayout,
            surfaceTextureView,
            GraphicsDevice.PointSampler
        ));

        uint[] framebuffer = new uint[window.Size.X * window.Size.Y];

        Array.Fill(framebuffer, 0x00000000u);

        GraphicsDevice.UpdateTexture(surfaceTexture, framebuffer, 0, 0, 0, (uint)window.Size.X, (uint)window.Size.Y, 1, 0, 0);
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
    public void Update(double dt)
    {
        _ = dt;

        window.Title = $"CelticCode - {dt}";

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
        commandList.SetGraphicsResourceSet(0, textureSet);
        commandList.DrawIndexed(4, 1, 0, 0, 0);

        commandList.End();
        GraphicsDevice.SubmitCommands(commandList);
        GraphicsDevice.WaitForIdle();

        GraphicsDevice.SwapBuffers();
    }

    public void Resize(Vector2D<int> size)
    {
        GraphicsDevice.MainSwapchain.Resize((uint)size.X, (uint)size.Y);

        UpdateFramebuffer();

        window.DoUpdate();
        window.DoRender();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        textureSet.Dispose();
        surfaceTextureView.Dispose();
        surfaceTexture.Dispose();

        pipeline.Dispose();

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
