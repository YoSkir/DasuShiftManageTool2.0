using DasuShiftManager.Code.Entities;

namespace DasuShiftManager.Code.Models;

public class MonthlyShiftModelDFS :IMonthlyShiftModel
{
    //排入員工歷史紀錄，方便遞迴時回溯狀態
    private readonly Stack<AssignMove> _assignHistory = new();
    
    //這裡把字典的獲取另外抽離，減少獲取內容時檢查的程式碼，並且將未來可能的檢查與錯誤處理留下擴充空間
    //已排的每日半時員工數
    private readonly Dictionary<DateOnly, int[]> _monthHalfHrStaffCounts = new();
    private int[] _getDailyHHSC(DateOnly date)
    {
        if (_monthHalfHrStaffCounts.TryGetValue(date, out var hhsc) || hhsc == null)
        {
            throw new InvalidOperationException($"Date {date.ToShortDateString()} half hour staff counts not found");
        }
        return hhsc;
    }
    //員工id對應的當月排班狀態
    private readonly Dictionary<int,StaffShift> _staffShifts = new();
    private StaffShift _getStaffShift(int staffId)
    {
        if (_staffShifts.TryGetValue(staffId, out var staffShift)||staffShift==null)
        {
            throw new InvalidOperationException($"Staff id: {staffId} shift not found");
        }
        return staffShift;
    }
    
    public MonthlyShiftModelDFS(DateOnly firstDay, Setting setting, List<Staff> staffs)
    {
        foreach (var staff in staffs)
        {
            _staffShifts[staff.Id] = new StaffShift();
        }

        var lastDay = firstDay.AddMonths(1);
        while (firstDay <= lastDay)
        {
            _monthHalfHrStaffCounts[firstDay] = new int[setting.ShiftHalfHrCount];
            firstDay.AddDays(1);
        }
    }

    public bool AssignStaff(DateOnly date, int staffId, int startArrHalfHr, int workHalfHrs, StaffType staffType)
    {
        //班別是否會溢出已在遞迴處檢查 因為還有固定班別 所以這裡可以考慮溢出不管 直接排到底 或是再做一次溢出檢查
        throw new NotImplementedException();
    }

    public bool AssignStaff(DateOnly date, int staffId)
    {
        throw new NotImplementedException();
    }

    public int GetWorkerCount(DateOnly date, int halfHr)
    {
        throw new NotImplementedException();
    }

    public void UnassignStaff()
    {
        throw new NotImplementedException();
    }

    public bool IsStaffAlreadyAssigned(DateOnly date, int staffId)
    {
        return _getStaffShift(staffId).IsAlreadyAssigned(date);
    }
}

public class AssignMove(DateOnly date,int staffId)
{
    public DateOnly Date { get; init; } = date;
    public int StaffId { get; init; }=staffId;
}

public class StaffShift
{
    public readonly Dictionary<DateOnly, DailyShift> MonthShift = new();

    public bool IsAlreadyAssigned(DateOnly date)
    {
        return MonthShift.ContainsKey(date);
    }
}