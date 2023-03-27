namespace CelticCode.Renderer;

using System.Collections.Generic;
using System.Numerics;

using CelticCode.Extension;
using CelticCode.Freetype;

using Veldrid;

public class GeometryBuffer
{
    private List<Vertex> vertices = new();
    private List<uint> indices = new();

    private DeviceBuffer vertexBuffer;
    private DeviceBuffer indexBuffer;
    private readonly GraphicsDevice graphicsDevice;

    public GeometryBuffer(GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
        Clear();
    }

    public void Upload()
    {
        vertexBuffer?.Dispose();
        indexBuffer?.Dispose();

        uint sizeInBytes;

        sizeInBytes = (uint)(vertices.Count * Vertex.SizeInBytes);
        vertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(sizeInBytes, BufferUsage.VertexBuffer));

        sizeInBytes = (uint)(indices.Count * sizeof(uint));
        indexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(sizeInBytes, BufferUsage.IndexBuffer));

        // TODO: Only update modified text
        graphicsDevice.UpdateBuffer(vertexBuffer, 0, vertices.ToArray());
        graphicsDevice.UpdateBuffer(indexBuffer, 0, indices.ToArray());
    }

    public void AddQuad(Vector2 pos, Vector2 size, Vector2 uvPos, Vector2 uvSize)
    {
        uint offset = (uint)vertices.Count;

        RgbaFloat foreground = new(212 / 255f, 212 / 255f, 212 / 255f, 1f);
        RgbaFloat background = new(25 / 255f, 29 / 255f, 31 / 255f, 1f);

        vertices.Add(new Vertex(pos, uvPos, foreground, background));
        vertices.Add(new Vertex(pos + new Vector2(size.X, 0), uvPos + new Vector2(uvSize.X, 0), foreground, background));
        vertices.Add(new Vertex(pos + new Vector2(0, size.Y), uvPos + new Vector2(0, uvSize.Y), foreground, background));
        vertices.Add(new Vertex(pos + size, uvPos + uvSize, foreground, background));

        indices.Add(offset + 0);
        indices.Add(offset + 1);
        indices.Add(offset + 2);

        indices.Add(offset + 1);
        indices.Add(offset + 3);
        indices.Add(offset + 2);
    }

    public void GenerateFontGeometry(TextFile file, FontAtlas atlas)
    {
        uint w = atlas.Width;
        uint h = atlas.Height;

        float penX;
        float penY = 0;

        Line line = file.FirstLine;

        while (line != null)
        {
            penX = 0;

            for (int i = 0; i < line.Text.Length; i++)
            {
                if (atlas.Glyphs.TryGetValue(line.Text[i], out Glyph g))
                {
                    Vector2 pos = new(penX + g.TopLeft.X, penY + (g.Advance.Y - g.TopLeft.Y));
                    Vector2 uvPos = new(g.Offset, 0);
                    Vector2 uvSize = new(g.Size.X / w, g.Size.Y / h);

                    AddQuad(pos, g.Size, uvPos, uvSize);

                    penX += g.Advance.X;
                }
            }

            penY += atlas.LineHeight;

            line = line.NextLine;
        }
    }

    public void Clear()
    {
        vertices.Clear();
        vertices.Add(new());

        indices.Clear();
        indices.Add(0);
        indices.Add(0);
        indices.Add(0);

        vertexBuffer?.Dispose();
        indexBuffer?.Dispose();
    }

    public void Draw(CommandList commandList)
    {
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt32);

        // Draw Quad
        commandList.DrawIndexed((uint)indices.Count, 1, 0, 0, 0);
    }
}
