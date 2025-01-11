namespace NeissDataParser;

public enum Disposition
{
    NoInjury = 0,
    TreatedAndReleased = 1,
    TreatedAndTransferred = 2,
    TreatedAndAdmitted = 4,
    HeldForObservation = 5,
    LeftWithoutBeingSeen = 6,
    Fatality = 8,
    Unknown = 9
}
