namespace CelticCode.Renderer;

using System.Collections.Generic;
using System.IO;

using Veldrid;
using Veldrid.SPIRV;

public class Mesh<T> where T : unmanaged
{
    private List<T> vertices = new();
    private List<uint> indices = new();

    private DeviceBuffer vertexBuffer;
    private DeviceBuffer indexBuffer;
    private Pipeline pipeline;
    private ResourceSet resourceSet;

    private const int MaxQuads = 10922;

    public Mesh(VeldridRenderer renderer, DeviceBuffer matrixBuffer, Texture texture)
    {
        uint vertSize = MaxQuads * 4 * Vertex.SizeInBytes;
        vertexBuffer = renderer.ResourceFactory.CreateBuffer(new BufferDescription(vertSize, BufferUsage.VertexBuffer));

        uint indexSize = MaxQuads * 6 * sizeof(uint);
        indexBuffer = renderer.ResourceFactory.CreateBuffer(new BufferDescription(indexSize, BufferUsage.IndexBuffer));

        Upload(renderer);

        VertexLayoutDescription vertexLayoutDescription = new(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Foreground", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("Background", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
        );

        Shader[] shaders = renderer.ResourceFactory.CreateFromSpirv(
            new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes("Assets/Shaders/Font.vert"), "main"),
            new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes("Assets/Shaders/Font.frag"), "main")
        );

        ShaderSetDescription shaderSet = new(
            new[] { vertexLayoutDescription },
            shaders
        );

        ResourceLayout textureLayout = renderer.ResourceFactory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Matrix", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
            )
        );

        TextureView textureView = renderer.ResourceFactory.CreateTextureView(texture);

        resourceSet = renderer.ResourceFactory.CreateResourceSet(new ResourceSetDescription(textureLayout, matrixBuffer, textureView, renderer.GraphicsDevice.PointSampler));

        pipeline = renderer.ResourceFactory.CreateGraphicsPipeline(
            new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                VeldridRenderer.RasterizerState,
                PrimitiveTopology.TriangleList,
                shaderSet,
                new[] { textureLayout },
                renderer.Swapchain.Framebuffer.OutputDescription
            )
        );
    }

    public void Upload(VeldridRenderer renderer)
    {
        renderer.GraphicsDevice.UpdateBuffer(vertexBuffer, 0, vertices.ToArray());
        renderer.GraphicsDevice.UpdateBuffer(indexBuffer, 0, indices.ToArray());
    }

    public void AddQuad(T tl, T tr, T bl, T br)
    {
        uint offset = (uint)vertices.Count;

        vertices.Add(tl);
        vertices.Add(tr);
        vertices.Add(bl);
        vertices.Add(br);

        indices.Add(offset + 0);
        indices.Add(offset + 1);
        indices.Add(offset + 2);

        indices.Add(offset + 1);
        indices.Add(offset + 3);
        indices.Add(offset + 2);
    }

    public void Render(CommandList commandList)
    {
        commandList.SetPipeline(pipeline);
        commandList.SetGraphicsResourceSet(0, resourceSet);
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt32);
        commandList.DrawIndexed((uint)indices.Count, 1, 0, 0, 0);
    }
}
