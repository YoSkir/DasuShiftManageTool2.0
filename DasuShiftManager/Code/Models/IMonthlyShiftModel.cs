namespace DasuShiftManager.Code.Models;

public interface IMonthlyShiftModel
{
    bool IsDone();
    bool AddWorker(DateOnly date, int employeeId, int startHalfHour, int workHalfHours, bool isManager);
    bool AddDayOffWorker(DateOnly date, int employeeId);
    int GetWorkerCount(DateOnly date, int halfHour);
    //todo 生成結果的函示
}