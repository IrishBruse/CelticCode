namespace CelticCode.Renderer;

using System.Numerics;

using CelticCode.Extension;
using CelticCode.Freetype;

using Silk.NET.Maths;

using Veldrid;

public class VeldridRenderer
{
    public GraphicsDevice GraphicsDevice { get; private set; }
    public ResourceFactory ResourceFactory { get; private set; }
    public CommandList CommandList { get; private set; }
    public Swapchain Swapchain { get; private set; }

    private FontAtlas fontAtlas;
    public static readonly RasterizerStateDescription RasterizerState = new(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, false, false);

    private static readonly Matrix4x4 ViewMatrix = Matrix4x4.CreateLookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);
    private DeviceBuffer matrixBuffer;

    private Mesh<Vertex> mesh;

    public VeldridRenderer(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;
        ResourceFactory = graphicsDevice.ResourceFactory;
        CommandList = ResourceFactory.CreateCommandList();
        Swapchain = graphicsDevice.MainSwapchain;

        fontAtlas = FontAtlas.GenerateSubpixelTexture(this, "Assets/Fonts/CascadiaCode.ttf", 12);

        matrixBuffer = ResourceFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

        mesh = new(this, matrixBuffer, fontAtlas.Texture);
    }

    public void GenerateFontGeometry(TextFile file, FontAtlas atlas)
    {
        uint w = atlas.Width;
        uint h = atlas.Height;

        float penX;
        float penY = 0;

        RgbaFloat foreground = new(212 / 255f, 212 / 255f, 212 / 255f, 1f);
        RgbaFloat background = new(25 / 255f, 29 / 255f, 31 / 255f, 1f);

        for (int i = 0; i < file.Lines; i++)
        {
            file.GetLine(i);
        }

        // while (line != null)
        // {
        //     penX = 0;

        //     for (int i = 0; i < line.Text.Length; i++)
        //     {
        //         if (atlas.Glyphs.TryGetValue(line.Text[i], out Glyph g))
        //         {
        //             Vector2 pos = new(penX + g.TopLeft.X, penY + (g.Advance.Y - g.TopLeft.Y));
        //             Vector2 uvPos = new(g.Offset, 0);
        //             Vector2 uvSize = new(g.Size.X / w, g.Size.Y / h);

        //             Vertex tl = new(pos, uvPos, foreground, background);
        //             Vertex tr = new(pos + new Vector2(g.Size.X, 0), uvPos + new Vector2(uvSize.X, 0), foreground, background);
        //             Vertex bl = new(pos + new Vector2(0, g.Size.Y), uvPos + new Vector2(0, uvSize.Y), foreground, background);
        //             Vertex br = new(pos + g.Size, uvPos + uvSize, foreground, background);

        //             mesh.AddQuad(tl, tr, bl, br);

        //             penX += g.Advance.X;
        //         }
        //     }

        //     penY += atlas.LineHeight;

        //     line = line.NextLine;
        // }
    }

    public void Resize(Vector2D<int> size)
    {
        // 0,0 in the top left
        Matrix4x4 projMat = Matrix4x4.CreateOrthographicOffCenter(0f, size.X, size.Y, 0f, -1f, 1f);
        Matrix4x4 mvp = ViewMatrix * projMat;

        CommandList.UpdateBuffer(matrixBuffer, 0, ref mvp);

        GraphicsDevice.MainSwapchain.Resize((uint)size.X, (uint)size.Y);
    }

    public void Draw()
    {
        CommandList.Begin();
        CommandList.SetFramebuffer(Swapchain.Framebuffer);
        CommandList.ClearColorTarget(0, new(25 / 255f, 29 / 255f, 31 / 255f, 1f));

        mesh.Render(CommandList);

        CommandList.End();
        GraphicsDevice.SubmitCommands(CommandList);
        GraphicsDevice.SwapBuffers(Swapchain);
    }

    public void FileContentChanged(TextFile file)
    {
        GenerateFontGeometry(file, fontAtlas);
        mesh.Upload(this);
    }
}
