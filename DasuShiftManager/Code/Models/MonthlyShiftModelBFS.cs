using DasuShiftManager.Code.Entities;

namespace DasuShiftManager.Code.Models;

public class MonthlyShiftModelBFS :IMonthlyShiftModel
{
    public MonthlyShiftModelBFS(DateOnly firstDay, Setting setting, List<Employee> employees)
    {
        
    }
    
    public bool IsDone()
    {
        throw new NotImplementedException();
    }

    public bool AddWorker(DateOnly date, int employeeId, int startHalfHour, int workHalfHours, bool isManager)
    {
        throw new NotImplementedException();
    }

    public bool AddDayOffWorker(DateOnly date, int employeeId)
    {
        throw new NotImplementedException();
    }

    public int GetWorkerCount(DateOnly date, int halfHour)
    {
        throw new NotImplementedException();
    }
    
}