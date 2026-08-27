
namespace DasuShiftManager.Core.Entities;

public class DailyShift
{
    public int ShiftId { get; init; }
    public Dictionary<int, ShiftInfo> StaffShifts { get; } = [];
}