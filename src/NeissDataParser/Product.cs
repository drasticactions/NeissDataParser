namespace NeissDataParser;

public class Product
{
    public int Code { get; set; }
    public string Name { get; set; }

    public override string ToString()
    {
        return $"{Code} - {Name}";
    }
}