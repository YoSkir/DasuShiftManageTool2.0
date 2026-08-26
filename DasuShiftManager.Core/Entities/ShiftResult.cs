
namespace DasuShiftManager.Core.Entities;

public class ShiftResult
{
    public int ShiftId { get; init; }
    public Dictionary<int, ShiftInfo> StaffShifts { get; } = [];
}