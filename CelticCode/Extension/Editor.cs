namespace CelticCode.Extension;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public class Editor
{
    public Action<TextFile> OnFileContentChanged { get; set; }

    private readonly List<Tab> tabs = new();
    private int tabIndex;

    private Tab CurrentTab => tabs[tabIndex];

    private List<IExtension> extensions = new();

    public void InsertTextAtCursors(char text)
    {
        if (CurrentTab == null)
        {
            return;
        }

        CurrentTab.Content.Insert(0, text);

        OnFileContentChanged.Invoke(CurrentTab.Content);
    }

    public void InsertNewlineAtCursors()
    {
        if (CurrentTab == null)
        {
            return;
        }

        CurrentTab.Content.Insert(0, '\n');

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

    public void LoadExtensions()
    {
        Type interfaceType = typeof(IExtension);

        foreach (string pluginFile in Directory.GetFiles("./Plugins", "*.dll"))
        {
            string fullPath = Path.GetFullPath(pluginFile);
            Assembly asm = Assembly.LoadFile(fullPath);

            Type[] plugins = asm.GetTypes().Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass).ToArray();

            if (plugins.Length > 1)
            {
                Console.Error.WriteLine($"More than 1 type extending IExtension skipping {pluginFile}!");
                continue;
            }

            if (plugins.Length == 0)
            {
                continue;
            }

            IExtension extension = (IExtension)Activator.CreateInstance(plugins[0]);
            extensions.Add(extension);
        }
    }
}
