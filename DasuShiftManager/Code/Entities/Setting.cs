namespace DasuShiftManager.Code.Entities;

public class Setting
{
    public int ShiftStartDay { get; init; }
    public TimeOnly ShiftStartTime { get; init; }
    public int ShiftHalfHourCount { get; init; }
    public int FirstBreakActiveWorkHalfHrs { get; init; }
    public int FirstBreakDurationHalfHrs {get; init;}
    public int SecondBreakActiveWorkHalfHrs { get; init; }
    public int SecondBreakDurationHalfHrs { get; init; }
    public List<int> ShiftHalfHourType { get; init; } = [12,16,20,24];
    public Dictionary<int, int> EveryHalfHourMinWorkers { get; init; } = new();
    public Dictionary<int, int> EveryHalfHourMinManagersOrPharmacist { get; init; } = new();
    public int MaxChainWorkDays { get; init; }
}