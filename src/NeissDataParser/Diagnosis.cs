namespace NeissDataParser;

public class Diagnosis
{
    public int Code { get; set; }
    public string Name { get; set; }

    public override string ToString()
    {
        return $"{Code} - {Name}";
    }
}