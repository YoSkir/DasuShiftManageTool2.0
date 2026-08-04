namespace DasuShiftManager.Code.Models;

public interface IMonthlyShiftModel
{
    bool IsDone();
    bool AssignStaff(DateOnly date, int staffId, int startHalfHour, int workHalfHours, bool isManager);
    bool AssignStaff(DateOnly date, int staffId);
    int GetWorkerCount(DateOnly date, int halfHour);
    void UnassignStaff();
    //todo 生成結果的函示
}