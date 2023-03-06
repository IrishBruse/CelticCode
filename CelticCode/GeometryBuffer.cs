namespace CelticCode;

using System.Collections.Generic;
using System.Numerics;

using Veldrid;

public class GeometryBuffer
{
    private List<Vertex> vertices = new();
    private List<ushort> indices = new();

    private DeviceBuffer vertexBuffer;
    private DeviceBuffer indexBuffer;

    public void Upload(GraphicsDevice gd)
    {
        vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(vertices.Count * Vertex.SizeInBytes), BufferUsage.VertexBuffer));
        indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(indices.Count * sizeof(ushort)), BufferUsage.IndexBuffer));

        gd.UpdateBuffer(vertexBuffer, 0, vertices.ToArray());
        gd.UpdateBuffer(indexBuffer, 0, indices.ToArray());
    }

    public void AddQuad(Vector2 pos, Vector2 size, Vector2 uvPos, Vector2 uvSize)
    {
        ushort offset = (ushort)vertices.Count;

        vertices.Add(new Vertex(pos, uvPos));
        vertices.Add(new Vertex(pos + new Vector2(size.X, 0), uvPos + new Vector2(uvSize.X, 0)));
        vertices.Add(new Vertex(pos + new Vector2(0, size.Y), uvPos + new Vector2(0, uvSize.Y)));
        vertices.Add(new Vertex(pos + size, uvPos + uvSize));

        indices.Add((ushort)(offset + 0));
        indices.Add((ushort)(offset + 1));
        indices.Add((ushort)(offset + 2));

        indices.Add((ushort)(offset + 1));
        indices.Add((ushort)(offset + 3));
        indices.Add((ushort)(offset + 2));
    }

    public void Draw(CommandList commandList)
    {
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);

        // Draw Quad
        commandList.DrawIndexed((uint)indices.Count, 1, 0, 0, 0);
    }
}

public record struct Vertex(Vector2 Position, Vector2 TexCoord)
{
    public static readonly int SizeInBytes = 4 * sizeof(float);

    public Vertex(float x, float y, float u, float v) : this(new(x, y), new(u, v)) { }
}
