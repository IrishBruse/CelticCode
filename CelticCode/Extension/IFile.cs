namespace CelticCode.Extension;

using System.IO;

public interface IFile
{
    void Load(StreamReader reader);
    void InsertAt(char text, int line, int column);
    void DeleteAt(char text, int line, int column);
    void NewLineAfter(Line line);
    void DeleteLine(Line line);
}
