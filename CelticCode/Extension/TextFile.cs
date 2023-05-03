namespace CelticCode.Extension;

using System.IO;
using System.Text;

public class TextFile
{
    public string Path { get; private set; }
    public int Lines { get; private set; } = 1;

    private Line FirstLine { get; set; } = new();

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
                Lines++;
            }
            else
            {
                line.Text.Append(letter);
            }
        }
    }

    public void Save(StreamWriter writer)
    {
        Line line = FirstLine;

        while (line != null)
        {
            writer.Write(line.Text);
            line = line.NextLine;
        }
    }

    public Line GetLine(int line)
    {
        Line currentLine = FirstLine;

        for (int i = 0; i < line; i++)
        {
            currentLine = currentLine.NextLine;
        }

        return currentLine;
    }

    public void Insert(int offset, char c)
    {

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
