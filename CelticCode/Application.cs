namespace CelticCode;

using System;
using System.Numerics;

using CelticCode.Freetype;

using RaylibSharp;

public class Application : IDisposable
{
    Shader shader;
    Font font;
    int lineHeight;

    Camera2D camera;
    Camera3D camera3d;

    string lines;
    (int, int) screenTextDimensions;

    Mesh mesh;
    Material matDefault = Raylib.LoadMaterialDefault();
    List<Vector3> vertices = [];
    List<ushort> indices = [];
    List<Vector2> uvs = [];
    ushort index = 0;

    public Application(string path)
    {
        lines = File.ReadAllText(path);
        font = FontAtlas.GenerateSubpixelTexture("Assets/Fonts/CascadiaCode.ttf", 12, out lineHeight, 5);

        shader = Raylib.LoadFragmentShader("Assets/Shaders/font.frag");
        Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "background"), new(0x191D1FFF));
        Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "foreground"), new(0xC0C0C0FF));

        camera.Target = Vector2.Zero;
        camera.Offset = new(Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2);
        camera.Zoom = 1.0f;

        camera3d = new()
        {
            Position = new(0.0f, 0.0f, 0.0f),   // Camera position
            Up = new(0.0f, 1.0f, 0.0f),         // Camera up vector (rotation towards target)
            Projection = CameraProjection.Orthographic      // Camera mode type
        };

        GenerateMesh();
    }

    public void Update(double dt)
    {
        _ = dt;

        screenTextDimensions = ((int)MathF.Ceiling(Raylib.GetScreenWidth() / font.Recs['a'].Width), (int)MathF.Ceiling(Raylib.GetScreenHeight() / (float)lineHeight));
        Key key;

        do
        {
            key = (Key)Raylib.GetKeyPressed();
            if (key != Key.Null && key > Key.Apostrophe && key < Key.Grave)
            {
                Console.WriteLine(key);
            }
        }
        while (key != Key.Null);
    }

    public void Render(double dt)
    {
        _ = dt;

        Raylib.BeginDrawing();
        {
            Raylib.ClearBackground(25, 29, 31);

            Raylib.BeginMode2D(camera);
            {
            }
            Raylib.EndMode2D();

            Raylib.BeginMode3D(camera3d);
            {
                Raylib.DrawMesh(mesh, matDefault, Matrix4x4.Identity);
            }
            Raylib.EndMode3D();

            string[] debugLines = [
                $"{camera.Target.X:000.0}, {camera.Target.Y:000.0}"
            ];

            for (int i = 0; i < debugLines.Length; i++)
            {
                Raylib.DrawText(debugLines[i], 10, 8 + (i * 20), 20, Color.DarkGreen);
            }
        }
        Raylib.EndDrawing();
    }

    public void GenerateMesh()
    {
        vertices.Clear();
        uvs.Clear();

        mesh.Dispose();

        AddQuad(new(0, 0, 0), new(100, 0, 0), new(100, 100, 0), new(0, 100, 0));

        mesh = new()
        {
            VertexCount = vertices.Count,
            TriangleCount = vertices.Count / 3,
            Vertices = vertices.ToArray(),
            TexCoords = uvs.ToArray()
        };

        Raylib.UploadMesh(ref mesh, false);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        AddVertex(c);
        AddVertex(b);
        AddVertex(a);
    }

    void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        AddTriangle(c, b, a);
        AddTriangle(d, c, a);
    }

    void AddVertex(Vector3 vert)
    {
        vertices.Add(vert);
        indices.Add(index++);
    }

}
