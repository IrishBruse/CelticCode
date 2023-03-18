namespace CelticCode;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public class GenerateIcon
{
    [Conditional("DEBUG")]
    public static void Generate()
    {
        StringBuilder output = new();

        output.AppendLine("namespace CelticCode;");
        output.AppendLine("");
        output.AppendLine("public class Icon");
        output.AppendLine("{");
        output.AppendLine("    public static readonly byte[] Data = new byte[]");
        output.AppendLine("    {");
        GenerateData(output);
        output.AppendLine("    };");
        output.AppendLine("}");

        File.WriteAllText(Environment.CurrentDirectory + "/Icon.cs", output.ToString());
    }

    private static void GenerateData(StringBuilder output)
    {
        Image<Rgba32> img = Image.Load<Rgba32>(File.ReadAllBytes("../Icon.png"));
        for (int y = 0; y < img.Height; y++)
        {
            output.Append("        ");
            for (int x = 0; x < img.Width; x++)
            {
                string value = $"{img[x, y].R},{img[x, y].G},{img[x, y].B},{img[x, y].A},";
                output.Append(value);
            }
            output.AppendLine("");
        }

    }
}
