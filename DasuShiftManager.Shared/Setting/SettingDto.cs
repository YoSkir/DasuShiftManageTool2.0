namespace DasuShiftManager.Server.Entity.Setting;

public class SettingDto
{
    public int ShiftStartDay { get; set; }
    public int ShiftStartTime { get; set; }
    public int ShiftEndTime { get; set; }
    public int FirstBreakActiveWorkHrs { get; set; }
    public int FirstBreakDurationHalfHrs {get; set;}
    public int SecondBreakActiveWorkHrs { get; set; }
    public int SecondBreakDurationHalfHrs { get; set; }
    public List<int> ShiftWorkHrType { get; set; } = [];
    public int[] EveryHalfHrMinWorkers { get; set; } = [];
    public int[] EveryHalfHrMinManagersOrPharmacist { get; set; } = [];
    public int MaxChainWorkDays { get; set; }
    public int MinWeekRestDays { get; set; }
    public int MinMonthWorkHrs { get; set; }
}