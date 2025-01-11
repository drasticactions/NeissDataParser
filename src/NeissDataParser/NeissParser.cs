namespace NeissDataParser;

public class NeissParser
{
    public static List<IncidentRecord> ParseTsvFile(string filePath)
    {
        var records = new List<IncidentRecord>();
        var lines = File.ReadAllLines(filePath);

        // Skip header row
        for (int i = 1; i < lines.Length; i++)
        {
            var fields = lines[i].Split('\t')
                .Select(f => f.Trim('"'))
                .ToArray();

            if (fields.Length < 25) continue;

            var record = new IncidentRecord
            {
                CaseNumber = int.Parse(fields[0]),
                TreatmentDate = DateTime.Parse(fields[1]),
                Age = int.Parse(fields[2]),
                Gender = (Gender)int.Parse(fields[3]),
                Race = (Race)int.Parse(fields[4]),
                OtherRace = fields[5],
                Hispanic = (Hispanic)int.Parse(string.IsNullOrEmpty(fields[6]) ? "0" : fields[6]),
                BodyPart = int.Parse(fields[7]),
                Diagnosis = int.Parse(fields[8]),
                DiagnosisName = DiagnosisCodeLookup.GetDiagnosis(int.Parse(fields[8])).Name,
                OtherDiagnosis = fields[9],
                BodyPart2 = string.IsNullOrEmpty(fields[10]) ? null : (int?)int.Parse(fields[10]),
                Diagnosis2 = string.IsNullOrEmpty(fields[11]) ? null : (int?)int.Parse(fields[11]),
                OtherDiagnosis2 = fields[12],
                Disposition = (Disposition)int.Parse(fields[13]),
                Location = (Location)int.Parse(fields[14]),
                FireInvolvement = int.Parse(fields[15]),
                Product1 = int.Parse(fields[16]),
                Product2 = int.Parse(fields[17]),
                Product3 = int.Parse(fields[18]),
                Product1Name = ProductCodeLookup.GetProduct(int.Parse(fields[16])).Name,
                Product2Name = ProductCodeLookup.GetProduct(int.Parse(fields[17])).Name,
                Product3Name = ProductCodeLookup.GetProduct(int.Parse(fields[18])).Name,
                Alcohol = int.Parse(string.IsNullOrEmpty(fields[19]) ? "0" : fields[19]),
                Drug = int.Parse(string.IsNullOrEmpty(fields[20]) ? "0" : fields[20]),
                Narrative = fields[21],
                Stratum = fields[22],
                PSU = int.Parse(string.IsNullOrEmpty(fields[23]) ? "0" : fields[23]),
                Weight = double.Parse(string.IsNullOrEmpty(fields[24]) ? "0" : fields[24]),
            };

            records.Add(record);
        }

        return records;
    }
}