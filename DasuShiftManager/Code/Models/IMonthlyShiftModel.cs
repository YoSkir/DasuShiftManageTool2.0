namespace DasuShiftManager.Code.Models;

public interface IMonthlyShiftModel
{
    bool IsMonthDone();
    bool IsDateDone(DateOnly date);
    int GetCurrentDateMinUndoneHalfHr(DateOnly date);
    bool AssignStaff(DateOnly date, int staffId, int startArrHalfHr, int workHalfHrs, bool isManager);
    bool AssignStaff(DateOnly date, int staffId);
    int GetWorkerCount(DateOnly date, int halfHr);
    void UnassignStaff();
    bool IsStaffAlreadyAssigned(DateOnly date, int staffId);
    //todo 生成結果的函示
}