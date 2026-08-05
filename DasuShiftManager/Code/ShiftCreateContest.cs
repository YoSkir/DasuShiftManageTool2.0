using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.Models;

namespace DasuShiftManager.Code;

public class ShiftCreateContest
{
    public DateOnly StartDate { get; init; }
    public Setting Setting { get; init; }
    public Dictionary<DateOnly, List<int>> VacationData { get; init; }
    public List<Staff> StaffList { get; init; }
    public IMonthlyShiftModel Msm { get; init; }
    public int IdCount { get; set; }
    public int MinShiftHalfHr { get; init; }

    public ShiftCreateContest(Setting setting,
        Dictionary<DateOnly, List<int>> vacationData, List<Staff> staffList, IMonthlyShiftModel msm,DateOnly startDate)
    {
        StartDate = startDate;
        Setting= setting;
        VacationData = vacationData;
        StaffList = staffList;
        Msm = msm;
        if(setting.ShiftHalfHrType==null||setting.ShiftHalfHrType.Count==0)
            throw new InvalidOperationException("ShiftHalfHrType is null or empty");
        MinShiftHalfHr = setting.ShiftHalfHrType.Min();
        IdCount = 0;
        if(setting.EveryHalfHrMinWorkers.Length!=setting.ShiftHalfHrCount)
            throw new InvalidOperationException("EveryHalfHrMinWorkers is not equal to half hr count");
    }

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