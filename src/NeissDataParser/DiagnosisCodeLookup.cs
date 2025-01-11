namespace NeissDataParser;

public static class DiagnosisCodeLookup
{
    private static Dictionary<int, string> _diagnosisCodes;

    public static void Initialize(string formatFilePath)
    {
        _diagnosisCodes = new Dictionary<int, string>();
        var lines = File.ReadAllLines(formatFilePath)
                        .Select(l => l.Split('\t')
                        .Select(f => f.Trim('"'))
                        .ToArray());

        foreach (var line in lines)
        {
            if (line[0] == "DIAG" && line.Length >= 4)
            {
                if (int.TryParse(line[1], out int code))
                {
                    _diagnosisCodes[code] = line[3];
                }
            }
        }
    }

    public static Diagnosis GetDiagnosis(int code)
    {
        if (_diagnosisCodes == null)
        {
            throw new InvalidOperationException("Diagnosis codes not initialized. Call Initialize() first.");
        }

        return new Diagnosis
        {
            Code = code,
            Name = _diagnosisCodes.TryGetValue(code, out string name) ? name : "Unknown Diagnosis"
        };
    }
}