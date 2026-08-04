namespace DasuShiftManager.Code.Entities;

public class Setting
{
    public int ShiftStartDay { get; init; }
    public int ShiftStartHalfHr { get; init; }
    public int ShiftHalfHrCount { get; init; }
    public int FirstBreakActiveWorkHalfHrs { get; init; }
    public int FirstBreakDurationHalfHrs {get; init;}
    public int SecondBreakActiveWorkHalfHrs { get; init; }
    public int SecondBreakDurationHalfHrs { get; init; }
    public List<int> ShiftHalfHrType { get; init; } = [12,16,20,24];
    public Dictionary<int, int> EveryHalfHrMinWorkers { get; init; } = new();
    public Dictionary<int, int> EveryHalfHrMinManagersOrPharmacist { get; init; } = new();
    public int MaxChainWorkDays { get; init; }
    public int MinWeekRestDays { get; init; }
    public int MinMonthWorkHrs { get; init; }
    public Dictionary<int, DailyShift[]> FixedShiftStaff { get; init; } = new();
}