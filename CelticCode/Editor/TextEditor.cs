namespace CelticCode.Editor;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a text editor that allows opening, creating and editing text files.
/// </summary>
public class TextEditor
{
    public required Action<TextFile> OnFileContentChanged { get; set; }

    readonly List<Tab> tabs = [];
    int tabIndex;

    Tab CurrentTab => tabs[tabIndex];

    public void InsertTextAtCursors(char text)
    {
        if (CurrentTab == null)
        {
            return;
        }

        TextFile.Insert(0, text);

        OnFileContentChanged.Invoke(CurrentTab.Content);
    }

    public void InsertNewlineAtCursors()
    {
        if (CurrentTab == null)
        {
            return;
        }

        TextFile.Insert(0, '\n');

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
