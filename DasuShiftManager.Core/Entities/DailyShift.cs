
namespace DasuShiftManager.Core.Entities;

public class DailyShift
{
    public int ShiftId { get; init; }
    public Dictionary<int, ShiftInfo> StaffShifts { get; } = [];
    public int[] StaffCount { get; init; }
    public int WeightedScore { get; set; } = 0;
}