namespace CelticCode.Extension;

using System.IO;
using System.Text;

public class TextFile
{
    public string Path { get; private set; }
    public Line FirstLine { get; set; } = new();

    public TextFile(string path)
    {
        Path = path;
    }

    public void Load(StreamReader reader)
    {
        Line line = FirstLine;

        while (!reader.EndOfStream)
        {
            char letter = (char)reader.Read();

            if (letter == '\n')
            {
                line.NextLine = new Line();
                line.NextLine.PreviousLine = line;
                line = line.NextLine;
            }
            else
            {
                line.Text.Append(letter);
            }
        }
    }

    public override string ToString()
    {
        StringBuilder builder = new();

        Line line = FirstLine;

        while (line != null)
        {
            builder.Append(line.Text);
            line = line.NextLine;
        }

        return builder.ToString();
    }
}
