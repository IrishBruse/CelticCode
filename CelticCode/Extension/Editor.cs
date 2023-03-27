namespace CelticCode.Extension;

using System;
using System.Collections.Generic;

public class Editor
{
    public Action<TextFile> OnFileContentChanged { get; set; }

    private readonly List<Tab> tabs = new();
    private int tabIndex;

    private Tab CurrentTab => tabs[tabIndex];

    private int line;
    private int column;

    public void InsertTextAtCursors(char text)
    {
        if (CurrentTab == null)
        {
            return;
        }

        CurrentTab.Content.InsertAt(text, line, column);

        column++;

        OnFileContentChanged.Invoke(CurrentTab.Content);
    }

    public void Open(string path)
    {
        Tab tab = new(path);

        tabs.Add(tab);
        tabIndex = tabs.Count - 1;

        OnFileContentChanged.Invoke(tab.Content);
    }

    public void NewFile()
    {
        Tab tab = new();

        tabs.Add(tab);
        tabIndex = tabs.Count - 1;

        OnFileContentChanged.Invoke(tab.Content);
    }
}
