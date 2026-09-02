using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.Shift;

/// <summary>
/// 以 DFS/回溯方式追蹤排班狀態的實作，負責保存每位員工與每日時段的分配紀錄。
/// </summary>
public class DfsShiftState :IShiftState
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
    public DfsShiftState(DateOnly firstDay,DateOnly lastDay, Setting setting, List<Staff> staffs)
    {
        foreach (var staff in staffs)
        {
            _staffShifts[staff.Id] = new StaffShift(firstDay,lastDay);
        }

        var date = firstDay;
        while (date <= lastDay)
        {
            _monthHalfHrStaffCounts[date] = new int[setting.ShiftHalfHrCount];
            date=date.AddDays(1);
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
        var hhsc = _getDailyHHSC(date);
        if (startArrHalfHr + workHalfHrs > hhsc.Length)
        {
            Console.WriteLine($"While staff {staffId} assign date {date.ToShortDateString()} half hr overflow");
            return false;
        }
        
        try
        {
            _getStaffShift(staffId).Assigned(date,startArrHalfHr,workHalfHrs);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
        var shiftInfo = _getStaffShift(staffId);
        return shiftInfo.GetThisWeekDayOff(date);
        // var c = 0;
        // var getDate = date;
        //
        // if (getDate.DayOfWeek == DayOfWeek.Sunday)
        // {
        //     if (shiftInfo.IsDayOff(getDate)) c++;
        //     getDate=getDate.AddDays(-1);
        // }
        //
        // while (getDate.DayOfWeek != DayOfWeek.Sunday)
        // {
        //     if (shiftInfo.IsDayOff(getDate)) c++;
        //     getDate=getDate.AddDays(-1);
        // }
        //
        // return c;
    }

    public int GetWorkHalfHrs(int staffId, DateOnly date, int countDays)
    {
        var workHalfHrs = 0;
        for (var i = 0; i < countDays; i++)
        {
            var shiftInfo = _getStaffShift(staffId);
            workHalfHrs += shiftInfo.GetWorkHalfHrs(date.AddDays(-i));
        }

        return workHalfHrs;
    }

    public ShiftInfo GetShiftCopy(int staffId, DateOnly date)
    {
        return _getStaffShift(staffId).GetShiftCopy(date);
    }

    public void AssignShift(Dictionary<int, ShiftInfo> shiftStaffShifts, DateOnly date)
    {
        foreach (var staffId in shiftStaffShifts.Keys)
        {
            var shiftInfo=shiftStaffShifts[staffId];
            if(shiftInfo.DayOff)
                _getStaffShift(staffId).AssignedDayOff(date);
            else
                _getStaffShift(staffId).Assigned(date, shiftInfo);
        }
    }

    public int GetTotalWorkHalfHrs(int staffId)
    {
        return _getStaffShift(staffId).TotalWorkHalfHrs;
    }

    public int GetTotalRestDays(int staffId)
    {
        return _getStaffShift(staffId).GetTotalRestDays();
    }

    public void AssignPto(int staffId)
    {
        _getStaffShift(staffId).TotalWorkHalfHrs += IShiftState.PtoHalfHr;
        _assignHistory.Push(new AssignMove(staffId,true));
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
            _assignHistory.Push(new AssignMove(date,staffId));
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
        if (lastMove.Pto)
        {
            _getStaffShift(lastMove.StaffId).TotalWorkHalfHrs -= IShiftState.PtoHalfHr;
            return;
        }
        var shiftInfo=_getStaffShift(lastMove.StaffId).Unassigned(lastMove.Date);
        if(shiftInfo.DayOff) return;
        var hhsc = _getDailyHHSC(lastMove.Date);
        for (var i = 0; i < shiftInfo.WorkHalfHrs; i++)
        {
            hhsc[shiftInfo.StartArrHalfHr + i]--;
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
public class AssignMove
{
    public DateOnly Date { get;}
    public int StaffId { get;}
    public bool Pto { get; }

    public AssignMove(DateOnly date, int staffId)
    {
        Date = date;
        StaffId = staffId;
        Pto = false;
    }

    public AssignMove(int staffId, bool pto)
    {
        StaffId = staffId;
        Pto = true;
    }
}