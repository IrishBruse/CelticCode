namespace CelticCode;

using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;

using CelticCode.Font;

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

    private DeviceBuffer vertexBuffer;
    private DeviceBuffer indexBuffer;
    private DeviceBuffer matrixBuffer;
    private Shader[] shaders;
    private Pipeline pipeline;
    private Texture fontAtlasTexture;
    private TextureView surfaceTextureView;
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

        Stopwatch timer = Stopwatch.StartNew();
        FontGenerator.Generate(GraphicsDevice, ref fontAtlasTexture);
        timer.Stop();
        Console.WriteLine(timer.ElapsedMilliseconds);

        float w = fontAtlasTexture.Width;
        float h = fontAtlasTexture.Height;

        float[] verts = new[] {
            0f, 0f, 0.0f, 0.0f,
            w,  0f, 1.0f, 0.0f,
            0f, h,  0.0f, 1.0f,
            w,  h,  1.0f, 1.0f
        };

        ushort[] indexs = new ushort[] {
            0, 1, 2,
            1, 3, 2
        };

        vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(verts.Length * sizeof(float)), BufferUsage.VertexBuffer));
        indexBuffer = factory.CreateBuffer(new BufferDescription((uint)(indexs.Length * sizeof(ushort)), BufferUsage.IndexBuffer));

        GraphicsDevice.UpdateBuffer(vertexBuffer, 0, verts);
        GraphicsDevice.UpdateBuffer(indexBuffer, 0, indexs);

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
            new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
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

        Resize(window.Size);
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
        window.Title = $"CelticCode - {dt}";
    }

    public void Draw(double dt)
    {
        commandList.Begin();
        commandList.SetFramebuffer(GraphicsDevice.MainSwapchain.Framebuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(25 / 255f, 29 / 255f, 31 / 255f, 1f));

        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
        commandList.SetPipeline(pipeline);
        commandList.SetGraphicsResourceSet(0, textureSet);

        // Draw Quad
        commandList.DrawIndexed(6, 1, 0, 0, 0);

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
