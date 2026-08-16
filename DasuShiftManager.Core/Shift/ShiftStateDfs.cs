using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.Shift;

public class ShiftStateDfs :IShiftState
{
    //排入員工歷史紀錄，方便遞迴時回溯狀態
    private readonly Stack<AssignMove> _assignHistory = new();
    
    //這裡把字典的獲取另外抽離，減少獲取內容時檢查的程式碼，並且將未來可能的檢查與錯誤處理留下擴充空間
    //已排的每日半時員工數
    private readonly Dictionary<DateOnly, int[]> _monthHalfHrStaffCounts = new();
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
    private StaffShift _getStaffShift(int staffId)
    {
        if (!_staffShifts.TryGetValue(staffId, out var staffShift))
        {
            throw new InvalidOperationException($"Staff id: {staffId} shift not found");
        }
        return staffShift;
    }
    
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

    public int GetArrHalfHrAssignedStaffCount(DateOnly date, int arrHalfHr)
    {
        return _getDailyHHSC(date)[arrHalfHr];
    }

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

    public bool IsStaffAlreadyAssigned(DateOnly date, int staffId)
    {
        return _getStaffShift(staffId).IsAlreadyAssigned(date);
    }

    public int GetChainWorkDays(int staffId)
    {
        return _getStaffShift(staffId).ChainWorkDays;
    }
}

public class AssignMove(DateOnly date,int staffId)
{
    public DateOnly Date { get; init; } = date;
    public int StaffId { get; init; }=staffId;
}

public class StaffShift
{
    private readonly Dictionary<DateOnly, ShiftInfo> _monthShift = new();
    public int TotalWorkHalfHrs { get; set; }
    public int ChainWorkDays { get; set; }

    public bool IsAlreadyAssigned(DateOnly date)
    {
        return _monthShift.ContainsKey(date);
    }

    public void AssignedDayOff(DateOnly date) 
    {
        if(IsAlreadyAssigned(date))
            throw new InvalidOperationException($"Date {date.ToShortDateString()} is already assigned");
        _monthShift.Add(date,new ShiftInfo());
        ChainWorkDays = 0;
    }

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

    public bool IsDayOff(DateOnly date)
    {
        //因為用於遞迴途中往回檢查一周放假天數 所以null也會算放假 故不適合往未來查
        return !_monthShift.TryGetValue(date, out var shiftInfo) || shiftInfo.DayOff;
    }
}