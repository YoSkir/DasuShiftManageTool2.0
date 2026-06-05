namespace DasuShiftManager.Code.Entities;

public class Setting
{
    public int ShiftStartDay { get; set; }
    public TimeOnly ShiftStartTime { get; set; }
    public int ShiftHalfHourCount { get; set; }
    public int FirstBreakActiveWorkHalfHrs { get; set; }
    public int FirstBreakDurationHalfHrs {get; set;}
    public int SecondBreakActiveWorkHalfHrs { get; set; }
    public int SecondBreakDurationHalfHrs { get; set; }
    public List<int> ShiftHalfHourType { get; set; } = [12,16,20,24];
    public Dictionary<int, int> EveryHalfHourMinWorkers { get; set; } = new();
    public Dictionary<int, int> EveryHalfHourMinManagersOrPharmacist { get; set; } = new();
    public int MaxChainWorkDays { get; set; }
}