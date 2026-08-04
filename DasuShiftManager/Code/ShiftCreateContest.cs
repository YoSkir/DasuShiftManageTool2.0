using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.Models;

namespace DasuShiftManager.Code;

public class ShiftCreateContest(Setting setting,
    Dictionary<DateOnly, List<int>> vacationData, List<Staff> staffList, IMonthlyShiftModel msm,DateOnly startDate)
{
    public DateOnly StartDate { get; init; } = startDate;
    public Setting Setting { get; init; } = setting;
    public Dictionary<DateOnly, List<int>> VacationData { get; init; } = vacationData;
    public List<Staff> StaffList { get; init; } = staffList;
    public IMonthlyShiftModel Msm { get; init; } = msm;
    public int IdCount { get; set; }

    public ShiftCreateResult GenerateResult()
    {
        return new ShiftCreateResult(IdCount);
    }
    
    public List<Staff> GetAvailableStaffs(DateOnly date)
    {
        //排除固定班別員工
        var staffUnfixed = StaffList.Where(staff => !Setting.FixedShiftStaff.ContainsKey(staff.Id)).ToList();
        //排除排假員工
        var onWorkStaff=VacationData.TryGetValue(date, out var offStaffIds)
            ? [.. staffUnfixed.Where(staff => offStaffIds.Contains(staff.Id))]
            : staffUnfixed;
        //排除當日已排班員工
        return [.. onWorkStaff.Where(staff => !Msm.IsStaffAlreadyAssigned(date, staff.Id))];
    }
}

public class ShiftCreateResult(int resultCount)
{
    public int ResultCount { get; init; } = resultCount;
}