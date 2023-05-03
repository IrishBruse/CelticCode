namespace CelticCode.Extension;

using System.Collections.Generic;
using System.IO;

public class Tab
{
    public TextFile Content { get; private set; }

    private int scrollLine;
    private List<Cursor> cursors = new();

    /// <summary> Open new tab to file </summary>
    public Tab(string path)
    {
        Content = new(path);
        Content.Load(File.OpenText(path));

        cursors.Add(new Cursor(0, 0));
    }

    /// <summary> Open a new tab to file an already open file </summary>
    public Tab(TextFile content)
    {
        Content = content;

        cursors.Add(new Cursor(0, 0));
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
