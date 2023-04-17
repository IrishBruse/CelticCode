namespace CelticCode.Extension;

public class Cursor
{
    public Line Line { get; set; }
    public int Column { get; set; }

    public Cursor(Line line)
    {
        Line = line;
    }
}
