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

        cursors.Add(new Cursor(Content.FirstLine));
    }

    /// <summary> Open a new tab to file an already open file </summary>
    public Tab(TextFile content)
    {
        Content = content;

        cursors.Add(new Cursor(Content.FirstLine));
    }

    public void Insert(char text)
    {
        foreach (Cursor c in cursors)
        {
            c.Line.Text.Insert(c.Column, text);
            c.Column++;
        }
    }

    public void InsertNewLine()
    {
        for (int i = cursors.Count - 1; i >= 0; i--)
        {
            Cursor c = cursors[i];

            c.Line.NextLine = new Line();
            c.Line.NextLine.PreviousLine = c.Line;
            c.Line = c.Line.NextLine;
            c.Column = 0;
        }
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
