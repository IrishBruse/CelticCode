namespace CelticCode.Editor;

public class Cursor(int line, int column)
{
    public int Line { get; set; } = line;
    public int Column { get; set; } = column;
}
