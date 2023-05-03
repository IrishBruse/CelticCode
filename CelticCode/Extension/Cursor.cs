namespace CelticCode.Extension;

public class Cursor
{
    public int Line { get; set; }
    public int Column { get; set; }

    public Cursor(int line, int column)
    {
        Line = line;
        Column = column;
    }
}
