namespace CelticCode.Renderer;

using System.IO;
using System.Numerics;

using CelticCode.Freetype;

using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;

using Veldrid;
using Veldrid.SPIRV;

public class VeldridManager
{
    private IWindow window;

    public GraphicsDevice GraphicsDevice { get; private set; }

    private ResourceFactory factory;
    private CommandList commandList;
    private Shader[] shaders;
    private Pipeline pipeline;
    private DeviceBuffer matrixBuffer;
    private ResourceSet textureSet;
    private ResourceLayout textureLayout;

    private Matrix4x4 translationMatrix = Matrix4x4.Identity;
    private static readonly Matrix4x4 ViewMatrix = Matrix4x4.CreateLookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);

    private TextureView textureView;

    public VeldridManager(IWindow window)
    {
        this.window = window;
        GraphicsDevice = CreateGraphicsDevice();
        factory = GraphicsDevice.ResourceFactory;
        commandList = factory.CreateCommandList();
    }

    public void Setup(FontAtlas fontAtlas)
    {
        LoadShaders();
        CreatePipeline();

        matrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

        textureView = factory.CreateTextureView(fontAtlas.Texture);
        textureSet = factory.CreateResourceSet(new ResourceSetDescription(textureLayout, matrixBuffer, textureView, GraphicsDevice.PointSampler));
    }

    private void CreatePipeline()
    {
        GraphicsPipelineDescription pipelineDescription = new();
        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

        pipelineDescription.DepthStencilState = new DepthStencilStateDescription(true, true, ComparisonKind.LessEqual);
        RasterizerStateDescription rasterState = new(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
        pipelineDescription.RasterizerState = rasterState;
        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;

        VertexLayoutDescription vertexLayout = new(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Foreground", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("Background", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
        );

        textureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
           new ResourceLayoutElementDescription("Matrix", ResourceKind.UniformBuffer, ShaderStages.Vertex),
           new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
           new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        pipelineDescription.ShaderSet = new ShaderSetDescription(
            new VertexLayoutDescription[]
            {
                vertexLayout
            },
            shaders
        );

        pipelineDescription.ResourceLayouts = new[]
        {
            textureLayout
        };

        pipelineDescription.Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription;
        pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
    }

    public void UpdateCameraPosition(Vector3 position)
    {
        translationMatrix = Matrix4x4.CreateTranslation(position);
    }

    public void BeginDraw(float r, float g, float b)
    {
        commandList.Begin();

        commandList.SetFramebuffer(GraphicsDevice.MainSwapchain.Framebuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(r, g, b, 1f));

        commandList.SetPipeline(pipeline);
        commandList.SetGraphicsResourceSet(0, textureSet);
    }

    public void EndDraw()
    {
        commandList.End();

        GraphicsDevice.SubmitCommands(commandList);
        GraphicsDevice.WaitForIdle();
        GraphicsDevice.SwapBuffers();
    }

    private void LoadShaders()
    {
        ShaderDescription vertexShaderDesc = new(ShaderStages.Vertex, File.ReadAllBytes("Assets/Shaders/base.vert"), "main");
        ShaderDescription fragmentShaderDesc = new(ShaderStages.Fragment, File.ReadAllBytes("Assets/Shaders/base.frag"), "main");
        shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
    }

    private GraphicsDevice CreateGraphicsDevice()
    {
        GraphicsDeviceOptions graphicsOptions = new();
        graphicsOptions.SyncToVerticalBlank = true;

        return window.CreateGraphicsDevice(graphicsOptions);
    }

    public void Resize(Vector2D<int> size)
    {
        // 0,0 in the top left
        Matrix4x4 projMat = Matrix4x4.CreateOrthographicOffCenter(0f, size.X, size.Y, 0f, -1f, 1f);
        Matrix4x4 mvp = ViewMatrix * projMat * translationMatrix;

        commandList.UpdateBuffer(matrixBuffer, 0, ref mvp);

        GraphicsDevice.MainSwapchain.Resize((uint)size.X, (uint)size.Y);

        window.DoUpdate();
        window.DoRender();
    }

    public void Draw(GeometryBuffer textBuffer)
    {
        textBuffer.Draw(commandList);
    }
}
