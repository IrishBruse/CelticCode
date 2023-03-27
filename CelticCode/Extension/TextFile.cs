namespace CelticCode.Extension;

using System.IO;
using System.Text;

public class TextFile : IFile
{
    public Line FirstLine { get; set; } = new();

    public TextFile() { }

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
                line.Text += letter;
            }
        }
    }

    public void InsertAt(char text, int line, int column)
    {
        Line currentLine = FirstLine;

        for (int i = 0; i < line; i++)
        {
            currentLine = currentLine.NextLine;
        }

        currentLine.Text = currentLine.Text.Insert(column, text.ToString());
    }

    public override string ToString()
    {
        StringBuilder builder = new();

        Line line = FirstLine;

        while (line != null)
        {
            builder.AppendLine(line.Text);
            line = line.NextLine;
        }

        return builder.ToString();
    }

    public void DeleteAt(char text, int line, int column)
    {
        throw new System.NotImplementedException();
    }

    public void NewLineAfter(Line line)
    {
        throw new System.NotImplementedException();
    }

    public void DeleteLine(Line line)
    {
        throw new System.NotImplementedException();
    }
}
