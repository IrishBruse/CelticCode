namespace CelticCode;

using System;
using System.IO;
using System.Numerics;

using CelticCode.Freetype;

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

    private DeviceBuffer matrixBuffer;
    private Shader[] shaders;
    private Pipeline pipeline;
    private Texture fontAtlasTexture;
    private TextureView surfaceTextureView;
    private readonly IWindow window;

    private float scroll;
    private GeometryBuffer textBuffer = new();

    private ResourceLayout textureLayout;
    private ResourceSet textureSet;

    public Application(IWindow window)
    {
        this.window = window;
    }

    public void Load()
    {
        string file = File.ReadAllText(@"A:\CelticCode\test.txt");

        GraphicsDevice = CreateGraphicsDevice();
        factory = GraphicsDevice.ResourceFactory;
        commandList = factory.CreateCommandList();

        GenerateFontGeometry(file);

        ShaderDescription vertexShaderDesc = new(ShaderStages.Vertex, File.ReadAllBytes("Shaders/base.vert"), "main");
        ShaderDescription fragmentShaderDesc = new(ShaderStages.Fragment, File.ReadAllBytes("Shaders/base.frag"), "main");
        shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

        GraphicsPipelineDescription pipelineDescription = new();
        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

        pipelineDescription.DepthStencilState = new DepthStencilStateDescription(true, true, ComparisonKind.LessEqual);
        pipelineDescription.RasterizerState = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;

        VertexLayoutDescription vertexLayout = new(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Foreground", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("Background", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
        );

        pipelineDescription.ShaderSet = new ShaderSetDescription(new VertexLayoutDescription[] { vertexLayout }, shaders);

        textureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("MvpBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        pipelineDescription.ResourceLayouts = new[]
        {
            textureLayout
        };

        pipelineDescription.Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription;
        pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        HandleInput();

        matrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

        surfaceTextureView = factory.CreateTextureView(fontAtlasTexture);

        textureSet = factory.CreateResourceSet(new ResourceSetDescription(
            textureLayout,
            matrixBuffer,
            surfaceTextureView,
            GraphicsDevice.PointSampler
        ));

        textBuffer.Upload(GraphicsDevice);

        Resize(window.Size);
    }

    private void GenerateFontGeometry(string file)
    {
        FontGenerator.Generate(GraphicsDevice, ref fontAtlasTexture);
        uint w = fontAtlasTexture.Width;
        uint h = fontAtlasTexture.Height;

        // Atlas
        textBuffer.AddQuad(new(0, 0), new(w, h), new(0, 0), new(1, 1));

        float penX = 0;
        float penY = 0;

        float lineHeight = 14;

        foreach (char letter in file)
        {
            if (letter == '\r')
            {
                continue;
            }

            if (letter == '\n')
            {
                penX = 0;
                penY += lineHeight;
                lineHeight = 0;
                continue;
            }

            if (FontGenerator.Glyphs.TryGetValue(letter, out Glyph g))
            {
                Vector2 pos = new(penX + g.TopLeft.X, penY + (g.Advance.Y - g.TopLeft.Y));
                Vector2 uvPos = new(g.Offset, 0);
                Vector2 uvSize = new(g.Size.X / w, g.Size.Y / h);

                lineHeight = Math.Max(lineHeight, g.Advance.Y);

                textBuffer.AddQuad(pos, g.Size, uvPos, uvSize);

                penX += g.Advance.X;
            }
        }
    }

    private void HandleInput()
    {
        IInputContext input = window.CreateInput();
        IKeyboard keyboard = input.Keyboards[0];
        IMouse mouse = input.Mice[0];

        mouse.Scroll += (s, e) => scroll -= s.ScrollWheels[0].Y * 3;
    }

    public void Update(double dt)
    {
        window.Title = $"CelticCode - {dt}";
    }

    public void Draw(double dt)
    {
        _ = dt;

        commandList.Begin();
        {
            commandList.SetFramebuffer(GraphicsDevice.MainSwapchain.Framebuffer);
            commandList.ClearColorTarget(0, new RgbaFloat(25 / 255f, 29 / 255f, 31 / 255f, 1f));

            commandList.SetPipeline(pipeline);
            commandList.SetGraphicsResourceSet(0, textureSet);

            textBuffer.Draw(commandList);
        }
        commandList.End();

        GraphicsDevice.SubmitCommands(commandList);
        GraphicsDevice.WaitForIdle();
        GraphicsDevice.SwapBuffers();
    }

    public void Resize(Vector2D<int> size)
    {
        // 0,0 in the top left
        Matrix4x4 projMat = Matrix4x4.CreateOrthographicOffCenter(0f, size.X, size.Y, 0f, -1f, 1f);
        Matrix4x4 viewMat = Matrix4x4.CreateLookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);
        Matrix4x4 mvp = viewMat * projMat;

        commandList.UpdateBuffer(matrixBuffer, 0, ref mvp);

        GraphicsDevice.MainSwapchain.Resize((uint)size.X, (uint)size.Y);

        window.DoUpdate();
        window.DoRender();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        textureSet.Dispose();
        surfaceTextureView.Dispose();
        fontAtlasTexture.Dispose();

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
