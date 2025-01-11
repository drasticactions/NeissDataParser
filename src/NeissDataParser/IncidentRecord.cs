using SQLite;

namespace NeissDataParser;

public class IncidentRecord
{
    [PrimaryKey]
    public int CaseNumber { get; set; }
    public DateTime TreatmentDate { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public Race Race { get; set; }
    public string OtherRace { get; set; }
    public Hispanic Hispanic { get; set; }
    public int BodyPart { get; set; }
    public int Diagnosis { get; set; }

    public string DiagnosisName { get; set; }
    public string OtherDiagnosis { get; set; }
    public int? BodyPart2 { get; set; }
    public int? Diagnosis2 { get; set; }
    public string OtherDiagnosis2 { get; set; }
    public Disposition Disposition { get; set; }
    public Location Location { get; set; }
    public int FireInvolvement { get; set; }
    public int Product1 { get; set; }
    public int Product2 { get; set; }
    public int Product3 { get; set; }

    public string Product1Name { get; set; }

    public string Product2Name { get; set; }

    public string Product3Name { get; set; }

    public int Alcohol { get; set; }
    public int Drug { get; set; }
    public string Narrative { get; set; }
    public string Stratum { get; set; }
    public int PSU { get; set; }
    public double Weight { get; set; }

    public bool HasSeen { get; set; }

    public bool Blocked { get; set; }

    public override string ToString()
    {
        return $"Case: {CaseNumber}\n" +
                $"Date: {TreatmentDate:d}\n" +
                $"Patient: {Age}yo {Gender}, Race: {Race}\n" +
                $"Location: {Location}\n" +
                $"Disposition: {Disposition}\n" +
                $"Narrative: {Narrative}\n";
    }
}