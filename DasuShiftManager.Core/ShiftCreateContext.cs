using DasuShiftManager.Core.Data;
using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool.Filter;
using DasuShiftManager.Core.GenerateTool.ResultSaver;
using DasuShiftManager.Core.Shift;
using DasuShiftManager.Shared;

namespace DasuShiftManager.Core;

/// <summary>
/// 保存排班過程中需要共用的上下文資料與查詢邏輯。
/// </summary>
public class ShiftCreateContext
{
    public Dictionary<DateOnly, List<int>> PtoData { get; init; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public Setting Setting { get;  }
    public Dictionary<DateOnly, List<int>> VacationData { get; init; }
    public List<Staff> StaffList { get;  }
    public IShiftState ShiftState { get; set; }
    public int IdCount { get; set; }
    public IResultSaver ResultSaver { get; set; }
    //單日所有可能
    public List<DailyShift> DailyShift { get;} =[];
    public IShiftState? PrevShiftState { get; } = null;
    public Dictionary<int, ShiftInfo?[]> FixedShiftStaff { get; }
    public Dictionary<int,StaffPreferShift> PreferShift { get; }
    public ShiftType ShiftType { get; }
    public Dictionary<int, Dictionary<DateOnly, ShiftInfo>> AssignedShift { get; init; }
    public Dictionary<DayOfWeek,HalfHrWorkers> WeekHalfHrWorkers { get; }
    public IShiftFilter Filter { get; set; } = new EasyWeightedFilter();


    /// <summary>
    /// 建立排班上下文。
    /// </summary>
    /// <param name="setting">排班設定。</param>
    /// <param name="startDate">排班起始日期。</param>
    /// <param name="dataGetter">資料獲取器</param>
    /// <exception cref="InvalidOperationException">設定資料不合法時拋出。</exception>
    public ShiftCreateContext(Setting setting, DateOnly startDate,IDataGetter dataGetter)
    {
       StartDate = startDate;
       Setting = setting;
       StaffList = dataGetter.GetStaffList();
       WeekHalfHrWorkers = dataGetter.GetHalfHrWorkers();
       if(StaffList.Count==0)
           throw new InvalidOperationException("No staff list found");
       ShiftType = new ShiftType();
       ClearShiftTypeCount();
       FixedShiftStaff=dataGetter.GetFixedShift() ?? [];
       PreferShift = dataGetter.GetPreferShift() ?? [];
       if (setting.ShiftHalfHrType == null || setting.ShiftHalfHrType.Count == 0)
           throw new InvalidOperationException("ShiftHalfHrType is null or empty");
       IdCount = 0;
       if (setting.EveryHalfHrMinWorkers.Length != setting.ShiftHalfHrCount)
           throw new InvalidOperationException("EveryHalfHrMinWorkers is not equal to half hr count");
    }

    public void ClearShiftTypeCount()
    {
        foreach (var staff in StaffList)
        {
            ShiftType.All[staff.Id] = 0;
            ShiftType.Early[staff.Id] = 0;
            ShiftType.Late[staff.Id] = 0;
        }
    }

    /// <summary>
    /// 生成目前排班流程的最終結果物件。
    /// </summary>
    /// <returns>目前已生成的排班結果。</returns>
    public ShiftCreateResult GenerateResult()
    {
        return new ShiftCreateResult() { ResultCount = IdCount,Context = this};
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
           // where !Setting.FixedShiftStaff.ContainsKey(staff.Id)
           //排除排假員工
           // where offStaffIds == null || !offStaffIds.Contains(staff.Id)
           //排除連上天數已到上限員工
           // where ShiftState.GetChainWorkDays(staff.Id) < Setting.MaxChainWorkDays
           //排除當日已排班員工
           where !ShiftState.IsStaffAlreadyAssigned(date, staff.Id)
           //排除不符合每周放假天數員工
           // where MatchMinDayOff(date, staff.Id)
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
    public bool IsWorkerFull(DateOnly date, int arrHalfHr)
    {
        var currentWorkers = ShiftState.GetArrHalfHrAssignedStaffCount(date, arrHalfHr);
        var neededWorkers = Setting.EveryHalfHrMaxWorkers[arrHalfHr];
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
           DayOfWeek.Saturday => ShiftState.GetRestDaysOfCurrentWeek(staffId, date) >= 1,
           DayOfWeek.Sunday => ShiftState.GetRestDaysOfCurrentWeek(staffId, date) >= 2,
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

    public AssignableHrInfo GetAssignableHrInfo(DateOnly date)
    {
        var index = Setting.ShiftHalfHrCount;
        var count = 0;
        for (var i = 0; i < Setting.ShiftHalfHrCount; i++)
        {
            var currentWorkers = ShiftState.GetArrHalfHrAssignedStaffCount(date, i);
            var maxWorkers = Setting.EveryHalfHrMaxWorkers[i];
            if (maxWorkers > currentWorkers)
            {
                index=i;
                count++;
                break;
            }
        }
        for (var i = index+1; i < Setting.ShiftHalfHrCount; i++)
        {
            var currentWorkers = ShiftState.GetArrHalfHrAssignedStaffCount(date, i);
            var maxWorkers = Setting.EveryHalfHrMaxWorkers[i];
            if (maxWorkers <= currentWorkers)
            {
                break;
            }

            count++;
        }

        return new AssignableHrInfo(){Date = date,UndoneArrHalfHr = index,UndoneHalfHrCount = count};
    }

    public ShiftInfo GetShiftCopy(int staffId,DateOnly date)
    {
        return ShiftState.GetShiftCopy(staffId,date);
    }

    public void AssignShift(Dictionary<int, ShiftInfo> shiftStaffShifts, DateOnly date)
    {
        foreach (var staffId in shiftStaffShifts.Keys)
        {
            var info=shiftStaffShifts[staffId];
            AssignStaff(staffId, date, info);
        }
    }

    public void AssignStaff(int staffId,DateOnly date,ShiftInfo shiftInfo)
    {
        switch (shiftInfo.Type)
        {
            case Shared.ShiftType.Early:
                ShiftType.Early[staffId]++;
                break;
            case Shared.ShiftType.Late:
                ShiftType.Late[staffId]++;
                break;
            case Shared.ShiftType.All:
                ShiftType.All[staffId]++;
                break;
            case Shared.ShiftType.Rest:
                break;
        }
        ShiftState.AssignShift(staffId,date,shiftInfo);
    }

    public void RefreshUndoneInfo(AssignableHrInfo assignableHrInfo)
    {
        var newUndone = GetAssignableHrInfo(assignableHrInfo.Date);
        assignableHrInfo.UndoneHalfHrCount=newUndone.UndoneHalfHrCount;
        assignableHrInfo.UndoneArrHalfHr=newUndone.UndoneArrHalfHr; 
    }

    public int NextNotFullArrHalfHr(DateOnly date, int arrHalfHr)
    {
        for (var i = arrHalfHr; i < Setting.ShiftHalfHrCount; i++)
        {
            var currentWorkers = ShiftState.GetArrHalfHrAssignedStaffCount(date, i);
            var neededWorkers = Setting.EveryHalfHrMaxWorkers[i];
            if (neededWorkers > currentWorkers) return i;
        }
        return Setting.ShiftHalfHrCount;
    }
}

/// <summary>
/// 代表排班生成流程的最終輸出物件。
/// </summary>
public class ShiftCreateResult
{
    public int ResultCount { get; init; }
    public ShiftCreateContext Context { get; init; }
}

public class ShiftType
{
    public Dictionary<int, int> Early { get; } = [];
    public Dictionary<int, int> Late { get; } = [];
    public Dictionary<int, int> All { get; } = [];
}

public class AssignableHrInfo
{
    public DateOnly Date { get; init; }
    public int UndoneArrHalfHr { get; set; }
    public int UndoneHalfHrCount { get; set; }
}