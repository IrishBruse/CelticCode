namespace CelticCode;

using System.Collections.Generic;
using System.Numerics;

using Veldrid;

public class GeometryBuffer
{
    private List<Vertex> vertices = new();
    private List<uint> indices = new();

    private DeviceBuffer vertexBuffer;
    private DeviceBuffer indexBuffer;
    private readonly GraphicsDevice gd;

    public GeometryBuffer(GraphicsDevice gd)
    {
        this.gd = gd;
    }

    public void Upload()
    {
        vertexBuffer?.Dispose();
        indexBuffer?.Dispose();

        vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(vertices.Count * Vertex.SizeInBytes), BufferUsage.VertexBuffer));
        indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(indices.Count * sizeof(uint)), BufferUsage.IndexBuffer));

        gd.UpdateBuffer(vertexBuffer, 0, vertices.ToArray());
        gd.UpdateBuffer(indexBuffer, 0, indices.ToArray());
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

    public void Clear()
    {
        vertices.Clear();
        indices.Clear();
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
