using DasuShiftManager.Code.Entities;

namespace DasuShiftManager.Code.Models;

public class MonthlyShiftModelDFS :IMonthlyShiftModel
{
    //排入員工歷史紀錄，方便遞迴時回溯狀態
    private readonly Stack<AssignMove> _assignHistory = new();
    //todo 每日的arr只記人數，另有個員工當月班別，這樣回溯時只需要知道員工與日期 並且扣去人數
    
    public MonthlyShiftModelDFS(DateOnly firstDay, Setting setting, List<Staff> staffs)
    {
        
    }
    
    public bool IsDone()
    {
        throw new NotImplementedException();
    }

    public bool AssignStaff(DateOnly date, int staffId, int startHalfHour, int workHalfHours, bool isManager)
    {
        throw new NotImplementedException();
    }

    public bool AssignStaff(DateOnly date, int staffId)
    {
        throw new NotImplementedException();
    }

    public int GetWorkerCount(DateOnly date, int halfHour)
    {
        throw new NotImplementedException();
    }

    public void UnassignStaff()
    {
        throw new NotImplementedException();
    }
}

public class AssignMove(DateOnly date,int staffId)
{
    public DateOnly Date { get; init; } = date;
    public int StaffId { get; init; }=staffId;
}

public class StaffShift
{
    
}