namespace CelticCode.Extension;

using System.Text;

public class Line
{
    public Line NextLine { get; set; }
    public Line PreviousLine { get; set; }
    public StringBuilder Text { get; set; } = new();
}
