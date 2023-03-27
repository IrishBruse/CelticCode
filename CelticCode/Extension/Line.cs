namespace CelticCode.Extension;

public class Line
{
    public Line NextLine { get; set; }
    public Line PreviousLine { get; set; }
    public string Text { get; set; } = "";
}
