namespace DasuShiftManager.Core.Entities;

public class Setting
{
    public int ShiftStartDay { get; set; }
    public int ShiftStartHalfHr { get; init; }
    public int ShiftHalfHrCount { get; init; }
    public int FirstBreakActiveWorkHalfHrs { get; init; }
    public int FirstBreakDurationHalfHrs {get; init;}
    public int SecondBreakActiveWorkHalfHrs { get; init; }
    public int SecondBreakDurationHalfHrs { get; init; }
    public List<int> ShiftHalfHrType { get; init; } = [12,16,20,24];
    public int[] EveryHalfHrMinWorkers { get; init; } = [];
    public int[] EveryHalfHrMaxWorkers { get; init; } = [];
    public int[] EveryHalfHrMinManagersOrPharmacist { get; init; }=[];
    public int MaxChainWorkDays { get; init; }
    public int MinWeekRestDays { get; init; }
    public int MinMonthWorkHrs { get; set; }
    public int MinMonthRestDays { get; set; }
}