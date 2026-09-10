namespace DasuShiftManager.Shared;

public class ShiftGenerateDto
{
    public int Day { get; init; }
    public int MinWorkHrs { get; init; }
    public int MinRestDay{get; init; }
    public Dictionary<DateOnly, List<int>> PtoStaffList { get; init; } = [];
    public Dictionary<int, Dictionary<DateOnly, ShiftInfo>> AssignedShiftList { get; init; } = [];
    public Dictionary<DateOnly, List<int>> VacationList{ get; init; } = [];
}