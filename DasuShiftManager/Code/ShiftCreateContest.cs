using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.Shift;

namespace DasuShiftManager.Code;

public class ShiftCreateContest
{
    public DateOnly StartDate { get; init; }
    public Setting Setting { get; init; }
    public Dictionary<DateOnly, List<int>> VacationData { get; init; }
    public List<Staff> StaffList { get; init; }
    public IShiftState ShiftState { get; init; }
    public int IdCount { get; set; }
    public int MinShiftHalfHr { get; init; }

    public ShiftCreateContest(Setting setting,
        Dictionary<DateOnly, List<int>> vacationData, List<Staff> staffList, IShiftState shiftState, DateOnly startDate)
    {
        StartDate = startDate;
        Setting = setting;
        VacationData = vacationData;
        StaffList = staffList;
        ShiftState = shiftState;
        if (setting.ShiftHalfHrType == null || setting.ShiftHalfHrType.Count == 0)
            throw new InvalidOperationException("ShiftHalfHrType is null or empty");
        MinShiftHalfHr = setting.ShiftHalfHrType.Min();
        IdCount = 0;
        if (setting.EveryHalfHrMinWorkers.Length != setting.ShiftHalfHrCount)
            throw new InvalidOperationException("EveryHalfHrMinWorkers is not equal to half hr count");
    }

    public ShiftCreateResult GenerateResult()
    {
        return new ShiftCreateResult(IdCount);
    }

    public List<Staff> GetAvailableStaffs(DateOnly date)
    {
        var offStaffIds = VacationData.GetValueOrDefault(date);
        return
        [
            .. from staff in StaffList
            //排除固定班別員工
            where !Setting.FixedShiftStaff.ContainsKey(staff.Id)
            //排除排假員工
            where offStaffIds == null || !offStaffIds.Contains(staff.Id)
            //排除連上天數已到上限員工
            where ShiftState.GetChainWorkDays(staff.Id) < Setting.MaxChainWorkDays
            //排除當日已排班員工
            where !ShiftState.IsStaffAlreadyAssigned(date, staff.Id)
            //排除不符合每周放假天數員工
            where !NotMatchMinDayOff(date, staff.Id)
            select staff
        ];
    }

    private bool NotMatchMinDayOff(DateOnly date, int staffId)
    {
        //todo 這裡要用迴圈依照每周最低假日從週日往回判斷每天是否符合
        //每周檢查是否符合一周兩天假
        return date.DayOfWeek == DayOfWeek.Sunday &&
               ShiftState.GetVacationsOfCurrentWeek(staffId, date) < Setting.MinWeekRestDays;
    }
}

public class ShiftCreateResult(int resultCount)
{
    public int ResultCount { get; init; } = resultCount;
}