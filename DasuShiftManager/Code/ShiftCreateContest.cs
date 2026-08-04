using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.Models;

namespace DasuShiftManager.Code;

public class ShiftCreateContest(Setting setting,
    Dictionary<DateOnly, List<int>> vacationData, List<Staff> staffList, IMonthlyShiftModel msm)
{
    public Setting Setting { get; init; } = setting;
    public Dictionary<DateOnly, List<int>> VacationData { get; init; } = vacationData;
    public List<Staff> StaffList { get; init; } = staffList;
    public IMonthlyShiftModel Msm { get; init; } = msm;
    public int IdCount { get; set; }

    public ShiftCreateResult GenerateResult()
    {
        return new ShiftCreateResult(IdCount);
    }
}

public class ShiftCreateResult(int resultCount)
{
    public int ResultCount { get; init; } = resultCount;
}