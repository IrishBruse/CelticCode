namespace CelticCode.Editor;

using System.IO;
using System.Text;

public class TextFile(string path)
{
    public string Path { get; private set; } = path;
    public int Lines { get; private set; } = 1;

    private StringBuilder Content { get; set; } = new();

    public void Load(StreamReader reader)
    {
        Content.Append(reader.ReadToEnd());
    }

    public void Save(StreamWriter writer)
    {
        writer.Write(Content);
    }

    public string GetLine(int line)
    {
        for (int i = 0; i < Content.Length; i++)
        {
            if (Content[i] == '\n')
            {
                line--;
            }

            if (line == 0)
            {
                int start = i;

                while (i < Content.Length && Content[i] != '\n')
                {
                    i++;
                }

                return Content.ToString(start, i - start);
            }
        }

        return string.Empty;
    }

    public void Insert(int offset, char c)
    {
        _ = offset;
    }

    public override string ToString()
    {
        return Content.ToString();
    }
}
