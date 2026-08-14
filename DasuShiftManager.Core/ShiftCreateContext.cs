using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool.ResultSaver;
using DasuShiftManager.Core.Shift;

namespace DasuShiftManager.Core;

public class ShiftCreateContext
{
    public DateOnly StartDate { get; init; }
    public Setting Setting { get; init; }
    public Dictionary<DateOnly, List<int>> VacationData { get; init; }
    public List<Staff> StaffList { get; init; }
    public IShiftState ShiftState { get; init; }
    public int IdCount { get; set; }
    public int MinShiftHalfHr { get; init; }
    public IResultSaver ResultSaver { get; init; }

    public ShiftCreateContext(Setting setting,
        Dictionary<DateOnly, List<int>> vacationData, List<Staff> staffList, IShiftState shiftState, DateOnly startDate,IResultSaver resultSaver)
    {
        StartDate = startDate;
        Setting = setting;
        VacationData = vacationData;
        StaffList = staffList;
        ShiftState = shiftState;
        ResultSaver = resultSaver;
        if (setting.ShiftHalfHrType == null || setting.ShiftHalfHrType.Count == 0)
            throw new InvalidOperationException("ShiftHalfHrType is null or empty");
        MinShiftHalfHr = setting.ShiftHalfHrType.Min();
        IdCount = 0;
        if (setting.EveryHalfHrMinWorkers.Length != setting.ShiftHalfHrCount)
            throw new InvalidOperationException("EveryHalfHrMinWorkers is not equal to half hr count");
    }

    /**
     * 回傳最終結果 todo 目前尚不確定要回傳什麼
     */
    public ShiftCreateResult GenerateResult()
    {
        return new ShiftCreateResult();
    }

    /**
     * 用於遞迴排班時，依照當下State回傳可排員工列表
     */
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
            where MatchMinDayOff(date, staff.Id)
            select staff
        ];
    }

    public bool IsWorkerEnough(DateOnly date, int arrHalfHr)
    {
        var currentWorkers = ShiftState.GetArrHalfHrAssignedStaffCount(date, arrHalfHr);
        var neededWorkers = Setting.EveryHalfHrMinWorkers[arrHalfHr];
        return currentWorkers>=neededWorkers;
    }

    /**
     * 檢查員工在該日期時 當周是否已符合最低排假限制
     */
    private bool MatchMinDayOff(DateOnly date, int staffId)
    {
        //每周檢查是否符合一周假天數 基本上台灣勞基法是一周兩天 未來可能三天 所以目前直接寫死兩天判斷
        return date.DayOfWeek switch
        {
            DayOfWeek.Saturday => ShiftState.GetVacationsOfCurrentWeek(staffId, date) >= 1,
            DayOfWeek.Sunday => ShiftState.GetVacationsOfCurrentWeek(staffId, date) >= 2,
            _ => true
        };
    }
}

public class ShiftCreateResult
{
}