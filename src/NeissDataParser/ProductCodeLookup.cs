namespace NeissDataParser;

public static class ProductCodeLookup
{
    private static Dictionary<int, string> _productCodes;

    public static void Initialize(string formatFilePath)
    {
        _productCodes = new Dictionary<int, string>();
        var lines = File.ReadAllLines(formatFilePath)
                        .Select(l => l.Split('\t')
                        .Select(f => f.Trim('"'))
                        .ToArray());

        foreach (var line in lines)
        {
            if (line[0] == "PROD" && line.Length >= 4)
            {
                if (int.TryParse(line[1], out int code))
                {
                    _productCodes[code] = line[3];
                }
            }
        }
    }

    public static Product GetProduct(int code)
    {
        if (_productCodes == null)
        {
            throw new InvalidOperationException("Product codes not initialized. Call Initialize() first.");
        }

        return new Product
        {
            Code = code,
            Name = _productCodes.TryGetValue(code, out string name) ? name : "Unknown Product"
        };
    }

    public static List<Product> ListProducts()
    {
        if (_productCodes == null)
        {
            throw new InvalidOperationException("Product codes not initialized. Call Initialize() first.");
        }

        return _productCodes.Select(kvp => new Product { Code = kvp.Key, Name = kvp.Value }).ToList();
    }
}