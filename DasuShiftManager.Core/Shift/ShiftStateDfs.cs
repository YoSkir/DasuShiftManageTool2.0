using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.Shift;

/// <summary>
/// 以 DFS/回溯方式追蹤排班狀態的實作，負責保存每位員工與每日時段的分配紀錄。
/// </summary>
public class ShiftStateDfs :IShiftState
{
    //排入員工歷史紀錄，方便遞迴時回溯狀態
    private readonly Stack<AssignMove> _assignHistory = new();
    
    //這裡把字典的獲取另外抽離，減少獲取內容時檢查的程式碼，並且將未來可能的檢查與錯誤處理留下擴充空間
    //已排的每日半時員工數
    private readonly Dictionary<DateOnly, int[]> _monthHalfHrStaffCounts = new();

    /// <summary>
    /// 取得指定日期的半小時人力統計資料。
    /// </summary>
    /// <param name="date">要查詢的日期。</param>
    /// <returns>每日各半小時點位的人力統計陣列。</returns>
    /// <exception cref="InvalidOperationException">若該日期尚未初始化則拋出。</exception>
    private int[] _getDailyHHSC(DateOnly date)
    {
        if (!_monthHalfHrStaffCounts.TryGetValue(date, out var hhsc))
        {
            throw new InvalidOperationException($"Date {date.ToShortDateString()} half hour staff counts not found");
        }
        return hhsc;
    }
    //員工id對應的當月排班狀態
    private readonly Dictionary<int,StaffShift> _staffShifts = new();

    /// <summary>
    /// 取得指定員工的當月排班狀態資料。
    /// </summary>
    /// <param name="staffId">員工識別碼。</param>
    /// <returns>對應的 <see cref="StaffShift"/> 實例。</returns>
    /// <exception cref="InvalidOperationException">若員工不存在排班紀錄則拋出。</exception>
    private StaffShift _getStaffShift(int staffId)
    {
        if (!_staffShifts.TryGetValue(staffId, out var staffShift))
        {
            throw new InvalidOperationException($"Staff id: {staffId} shift not found");
        }
        return staffShift;
    }
    
    /// <summary>
    /// 初始化當月排班狀態。
    /// </summary>
    /// <param name="firstDay">當月起始日期。</param>
    /// <param name="setting">排班設定。</param>
    /// <param name="staffs">員工清單。</param>
    public ShiftStateDfs(DateOnly firstDay, Setting setting, List<Staff> staffs)
    {
        foreach (var staff in staffs)
        {
            _staffShifts[staff.Id] = new StaffShift();
        }

        var lastDay = firstDay.AddMonths(1);
        while (firstDay <= lastDay)
        {
            _monthHalfHrStaffCounts[firstDay] = new int[setting.ShiftHalfHrCount];
            firstDay=firstDay.AddDays(1);
        }
    }

    /// <summary>
    /// 將員工排入指定日期與時間段。
    /// </summary>
    /// <param name="date">日期。</param>
    /// <param name="staffId">員工識別碼。</param>
    /// <param name="startArrHalfHr">開始半小時索引。</param>
    /// <param name="workHalfHrs">工作半小時數。</param>
    /// <param name="staffType">員工類型。</param>
    /// <returns>若成功排班則返回 <see langword="true"/>。</returns>
    public bool AssignStaff(DateOnly date, int staffId, int startArrHalfHr, int workHalfHrs, StaffType staffType)
    {
        try
        {
            _getStaffShift(staffId).Assigned(date,startArrHalfHr,workHalfHrs);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        var hhsc = _getDailyHHSC(date);
        if (startArrHalfHr + workHalfHrs > hhsc.Length)
        {
            Console.WriteLine($"While staff {staffId} assign date {date.ToShortDateString()} half hr overflow");
            return false;
        }
        for (var i = 0; i < workHalfHrs; i++)
        {
            hhsc[startArrHalfHr + i]++;
        }
        _assignHistory.Push(new AssignMove(date,staffId));
        return true;
    }

    /// <summary>
    /// 取得指定員工在當周已排休假天數。
    /// </summary>
    /// <param name="staffId">員工識別碼。</param>
    /// <param name="date">用於定位週期的日期。</param>
    /// <returns>該週已休假的天數。</returns>
    public int GetVacationsOfCurrentWeek(int staffId, DateOnly date)
    {
        var c = 0;
        var shiftInfo = _getStaffShift(staffId);
        for (var i = 1; i < 6; i++)
        {
            if (shiftInfo.IsDayOff(date.AddDays(-i)))
                c++;
        }

        return c;
    }

    /// <summary>
    /// 將指定員工標記為休假。
    /// </summary>
    /// <param name="date">休假日期。</param>
    /// <param name="staffId">員工識別碼。</param>
    /// <returns>若休假標記成功則為 <see langword="true"/>。</returns>
    public bool AssignStaffDayOff(DateOnly date, int staffId)
    {
        try
        {
            _getStaffShift(staffId).AssignedDayOff(date);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"While staff {staffId} assign day off: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 取得指定日期與半小時點位的已排班人數。
    /// </summary>
    /// <param name="date">日期。</param>
    /// <param name="arrHalfHr">半小時索引。</param>
    /// <returns>該時段已分配的人數。</returns>
    public int GetArrHalfHrAssignedStaffCount(DateOnly date, int arrHalfHr)
    {
        return _getDailyHHSC(date)[arrHalfHr];
    }

    /// <summary>
    /// 回溯上一個排班動作，撤銷該動作的狀態變更。
    /// </summary>
    public void UnassignStaff()
    {
        var lastMove=_assignHistory.Pop();
        var shiftInfo=_getStaffShift(lastMove.StaffId).Unassigned(lastMove.Date);
        if(shiftInfo.DayOff) return;
        var hhsc = _getDailyHHSC(lastMove.Date);
        for (var i = 0; i < shiftInfo.WorkHalfHrs; i++)
        {
            hhsc[shiftInfo.StartHalfHr + i]--;
        }
    }

    /// <summary>
    /// 判斷指定員工在某日是否已被安排班別。
    /// </summary>
    /// <param name="date">日期。</param>
    /// <param name="staffId">員工識別碼。</param>
    /// <returns>若已排班則返回 <see langword="true"/>。</returns>
    public bool IsStaffAlreadyAssigned(DateOnly date, int staffId)
    {
        return _getStaffShift(staffId).IsAlreadyAssigned(date);
    }

    /// <summary>
    /// 取得指定員工目前的連續上班天數。
    /// </summary>
    /// <param name="staffId">員工識別碼。</param>
    /// <returns>連續上班的天數。</returns>
    public int GetChainWorkDays(int staffId)
    {
        return _getStaffShift(staffId).ChainWorkDays;
    }
}

/// <summary>
/// 保存一次排班回溯所需的日期與員工資訊。
/// </summary>
public class AssignMove(DateOnly date,int staffId)
{
    public DateOnly Date { get; init; } = date;
    public int StaffId { get; init; }=staffId;
}

/// <summary>
/// 保存單一員工在月份中的班表狀態。
/// </summary>
public class StaffShift
{
    private readonly Dictionary<DateOnly, ShiftInfo> _monthShift = new();
    public int TotalWorkHalfHrs { get; set; }
    public int ChainWorkDays { get; set; }

    /// <summary>
    /// 判斷指定日期是否已經存在班表紀錄。
    /// </summary>
    /// <param name="date">要檢查的日期。</param>
    /// <returns>若已排班或已排假則為 <see langword="true"/>。</returns>
    public bool IsAlreadyAssigned(DateOnly date)
    {
        return _monthShift.ContainsKey(date);
    }

    /// <summary>
    /// 為指定日期記錄休假狀態。
    /// </summary>
    /// <param name="date">休假日期。</param>
    /// <exception cref="InvalidOperationException">若該日期已被安排過則拋出。</exception>
    public void AssignedDayOff(DateOnly date) 
    {
        if(IsAlreadyAssigned(date))
            throw new InvalidOperationException($"Date {date.ToShortDateString()} is already assigned");
        _monthShift.Add(date,new ShiftInfo());
        ChainWorkDays = 0;
    }

    /// <summary>
    /// 為指定日期記錄正常工作班別。
    /// </summary>
    /// <param name="date">班表日期。</param>
    /// <param name="startHalfHr">開始的半小時索引。</param>
    /// <param name="workHalfHrs">工作時段長度（半小時）。</param>
    /// <exception cref="InvalidOperationException">若該日期已安排過則拋出。</exception>
    public void Assigned(DateOnly date, int startHalfHr, int workHalfHrs)
    {
        if(IsAlreadyAssigned(date))
            throw new InvalidOperationException($"Date {date.ToShortDateString()} is already assigned");
        _monthShift.Add(date,new ShiftInfo(startHalfHr,workHalfHrs));
        var lastDayShift = _monthShift?[date.AddDays(-1)];
        if (lastDayShift == null || lastDayShift.DayOff)
            ChainWorkDays = 0;
        ChainWorkDays++;
        TotalWorkHalfHrs+=workHalfHrs;
    }

    /// <summary>
    /// 撤銷指定日期的班表安排。
    /// </summary>
    /// <param name="date">要撤銷的日期。</param>
    /// <returns>被撤銷的班表資訊。</returns>
    /// <exception cref="InvalidOperationException">若該日期不存在排班紀錄則拋出。</exception>
    public ShiftInfo Unassigned(DateOnly date)
    {
        if (!_monthShift.Remove(date, out var shiftInfo))
        {
            throw new InvalidOperationException($"Date {date.ToShortDateString()} is not assigned");
        }

        if (!shiftInfo.DayOff)
        {
            TotalWorkHalfHrs-=shiftInfo.WorkHalfHrs;
            //這裡要注意 因為是遞迴呼叫總是最後一步才能這樣扣連續上班日
            ChainWorkDays = Math.Max(0, ChainWorkDays - 1);
        }
        return shiftInfo;
    }

    /// <summary>
    /// 判斷指定日期是否為休假日。
    /// </summary>
    /// <param name="date">日期。</param>
    /// <returns>若為休假或尚未安排排班，則返回 <see langword="true"/>。</returns>
    public bool IsDayOff(DateOnly date)
    {
        //因為用於遞迴途中往回檢查一周放假天數 所以null也會算放假 故不適合往未來查
        return !_monthShift.TryGetValue(date, out var shiftInfo) || shiftInfo.DayOff;
    }
}