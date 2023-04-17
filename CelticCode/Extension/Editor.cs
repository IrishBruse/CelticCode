namespace CelticCode.Extension;

using System;
using System.Collections.Generic;

public class Editor
{
    public Action<TextFile> OnFileContentChanged { get; set; }

    private readonly List<Tab> tabs = new();
    private int tabIndex;

    private Tab CurrentTab => tabs[tabIndex];

    public void InsertTextAtCursors(char text)
    {
        if (CurrentTab == null)
        {
            return;
        }

        CurrentTab.Insert(text);

        OnFileContentChanged.Invoke(CurrentTab.Content);
    }

    public void InsertNewlineAtCursors()
    {
        if (CurrentTab == null)
        {
            return;
        }

        CurrentTab.InsertNewLine();

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
        TextFile content = new(string.Empty);
        Tab tab = new(content);

        tabs.Add(tab);
        tabIndex = tabs.Count - 1;

        OnFileContentChanged.Invoke(tab.Content);
    }
}
