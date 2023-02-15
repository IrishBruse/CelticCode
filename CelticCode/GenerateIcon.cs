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
        output.AppendLine("public class Icon\n{");
        output.AppendLine("    public static readonly byte[] Data = new byte[]\n{");

        Image<Rgba32> img = Image.Load<Rgba32>(File.ReadAllBytes("Icon.png"));
        for (int y = 0; y < img.Height; y++)
        {
            for (int x = 0; x < img.Width; x++)
            {
                string value = $"{img[x, y].R}, {img[x, y].G}, {img[x, y].B}, {img[x, y].A},";
                output.Append(value);
            }
            output.AppendLine("");
        }

        output.AppendLine("\n    };");
        output.AppendLine("}");


        File.WriteAllText(Environment.CurrentDirectory + "/Icon.cs", output.ToString());
    }

}
