namespace CelticCode.Extension;
using System.IO;


public class Tab
{
    public string Path { get; private set; }
    public TextFile Content { get; private set; } = new();

    private int scrollLine;

    /// <summary> Open new tab to file </summary>
    public Tab(string path)
    {
        Path = path;
        Content.Load(File.OpenText(path));
    }

    /// <summary> Create new unsaved file </summary>
    public Tab()
    {

    }

    public void Scroll(int lines)
    {
        scrollLine += lines;

        if (scrollLine < 0)
        {
            scrollLine = 0;
        }
    }

    public void ScrollTo(int lines)
    {
        scrollLine = lines;

        if (scrollLine < 0)
        {
            scrollLine = 0;
        }
    }
}
