
namespace DasuShiftManager.Core.Entities;

public class DailyShift
{
    public int ShiftId { get; init; }
    public Dictionary<int, ShiftInfo> StaffShifts { get; } = [];
    public int[] StaffCount { get; init; }
    public int WeightedScore { get; set; } = 0;
    public WeightCount WeightCount { get; }=new WeightCount();
}

public class WeightCount
{
    public Dictionary<WeightType, int> Count { get; }=Enum.GetValues<WeightType>().ToDictionary(t => t, t => 0);
}

public enum WeightType
{
    連上天數,最低工時,偏好排班,早班平均
}