using DasuShiftManager.Code.Entities;

namespace DasuShiftManager.Code.Models;

public class MonthlyShiftModelDFS :IMonthlyShiftModel
{
    //排入員工歷史紀錄，方便遞迴時回溯狀態
    private readonly Stack<AssignMove> _assignHistory = new();
    //todo 每日的arr只記人數，另有個員工當月班別，這樣回溯時只需要知道員工與日期 並且扣去人數
    
    public MonthlyShiftModelDFS(DateOnly firstDay, Setting setting, List<Employee> employees)
    {
        
    }
    
    public bool IsDone()
    {
        throw new NotImplementedException();
    }

    public bool AssignWorker(DateOnly date, int employeeId, int startHalfHour, int workHalfHours, bool isManager)
    {
        throw new NotImplementedException();
    }

    public bool AssignWorker(DateOnly date, int employeeId)
    {
        throw new NotImplementedException();
    }

    public int GetWorkerCount(DateOnly date, int halfHour)
    {
        throw new NotImplementedException();
    }

    public void UnassignWorker()
    {
        throw new NotImplementedException();
    }
}

public class AssignMove(DateOnly date,int employeeId,int startHalfHour,int workHalfHours)
{
    public DateOnly Date { get; init; }
    public int EmployeeId { get; init; }
    public int StartHalfHour { get; init; }
    public int WorkHalfHours { get; init; }
}