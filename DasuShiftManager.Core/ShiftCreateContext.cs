using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool.ResultSaver;
using DasuShiftManager.Core.Shift;

namespace DasuShiftManager.Core;

/// <summary>
/// 保存排班過程中需要共用的上下文資料與查詢邏輯。
/// </summary>
public class ShiftCreateContext
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public Setting Setting { get;  }
    public Dictionary<DateOnly, List<int>> VacationData { get;  }
    public List<Staff> StaffList { get;  }
    public IShiftState ShiftState { get; set; }
    public int IdCount { get; set; }
    public IResultSaver ResultSaver { get; set; }
    public PruningStatement? PruningStatement{get; set;}

    /// <summary>
    /// 建立排班上下文。
    /// </summary>
    /// <param name="setting">排班設定。</param>
    /// <param name="vacationData">休假資料。</param>
    /// <param name="staffList">員工清單。</param>
    /// <param name="startDate">排班起始日期。</param>
    /// <exception cref="InvalidOperationException">設定資料不合法時拋出。</exception>
    public ShiftCreateContext(Setting setting,
       Dictionary<DateOnly, List<int>> vacationData, List<Staff> staffList, DateOnly startDate)
    {
       StartDate = startDate;
       Setting = setting;
       VacationData = vacationData;
       StaffList = staffList;
       if (setting.ShiftHalfHrType == null || setting.ShiftHalfHrType.Count == 0)
           throw new InvalidOperationException("ShiftHalfHrType is null or empty");
       IdCount = 0;
       if (setting.EveryHalfHrMinWorkers.Length != setting.ShiftHalfHrCount)
           throw new InvalidOperationException("EveryHalfHrMinWorkers is not equal to half hr count");
    }

    /// <summary>
    /// 生成目前排班流程的最終結果物件。
    /// </summary>
    /// <returns>目前已生成的排班結果。</returns>
    public ShiftCreateResult GenerateResult()
    {
        return new ShiftCreateResult() { ResultCount = IdCount };
    }

    /// <summary>
    /// 依照目前狀態，回傳某日可供排班的員工列表。
    /// </summary>
    /// <param name="date">要判斷是否可排的日期。</param>
    /// <returns>符合條件且仍可被安排的員工集合。</returns>
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

    /// <summary>
    /// 判斷指定時段是否已達到最低所需工作人數。
    /// </summary>
    /// <param name="date">日期。</param>
    /// <param name="arrHalfHr">半小時索引。</param>
    /// <returns>若已達到當前最低人力需求則為 <see langword="true"/>。</returns>
    public bool IsWorkerEnough(DateOnly date, int arrHalfHr)
    {
       var currentWorkers = ShiftState.GetArrHalfHrAssignedStaffCount(date, arrHalfHr);
       var neededWorkers = Setting.EveryHalfHrMinWorkers[arrHalfHr];
       return currentWorkers>=neededWorkers;
    }

    /// <summary>
    /// 檢查指定員工在當周是否已滿足最低排假限制。
    /// </summary>
    /// <param name="date">要判斷的日期。</param>
    /// <param name="staffId">員工識別碼。</param>
    /// <returns>若符合最少休假天數規則則為 <see langword="true"/>。</returns>
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

    public StaffType GetStaffType(int staffId)
    {
        var staff = StaffList.Find(staff => staff.Id == staffId);

        if (staff != null) return staff.StaffType;

        Console.WriteLine($"Cant find staff {staffId}'s type");
        return StaffType.Normal;
    }

    public int NextUndoneArrHalfHr(DateOnly date,int currentIndex = 0)
    {
        for (var i = currentIndex; i < Setting.ShiftHalfHrCount; i++)
        {
            var currentWorkers = ShiftState.GetArrHalfHrAssignedStaffCount(date, i);
            var neededWorkers = Setting.EveryHalfHrMinWorkers[i];
            if (neededWorkers > currentWorkers) return i;
        }
        return Setting.ShiftHalfHrCount;
    }

    public bool TooMuchDayOff(int id, DateOnly date)
    {
        if(PruningStatement==null) return false;
        return ShiftState.GetVacationsOfCurrentWeek(id, date) >= PruningStatement.MaxDayyOff;
    }

    public bool WorkHrGapTooBigPerWeek(DateOnly date)
    {
        if(PruningStatement==null) return false;
        var maxWh = int.MinValue;
        var minWh = int.MaxValue;
        foreach (var staff in StaffList)
        {
            var workHalfHrs = ShiftState.GetWorkHalfHrs(staff.Id, date, 8);
            maxWh = Math.Max(maxWh,workHalfHrs);
            minWh = Math.Min(minWh, workHalfHrs);
        }
        return maxWh - minWh > PruningStatement.MaxWorkHalfHrGap;
    }
}

/// <summary>
/// 代表排班生成流程的最終輸出物件。
/// </summary>
public class ShiftCreateResult
{
    public int ResultCount { get; set; } = 0;
}

public class PruningStatement
{
    public int MaxDayyOff { get; init; }
    public int MaxWorkHalfHrGap { get; init; }
    
    
}