namespace DasuShiftManager.Code.Models;

public interface IMonthlyShiftModel
{
    bool IsDone();
    bool AssignWorker(DateOnly date, int employeeId, int startHalfHour, int workHalfHours, bool isManager);
    bool AssignWorker(DateOnly date, int employeeId);
    int GetWorkerCount(DateOnly date, int halfHour);

    void UnassignWorker();
    //todo 生成結果的函示
}