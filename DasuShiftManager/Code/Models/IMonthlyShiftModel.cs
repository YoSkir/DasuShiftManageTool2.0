using DasuShiftManager.Code.Entities;

namespace DasuShiftManager.Code.Models;

public interface IMonthlyShiftModel
{
    bool AssignStaff(DateOnly date, int staffId, int startArrHalfHr, int workHalfHrs, StaffType staffType);
    bool AssignStaff(DateOnly date, int staffId);
    int GetWorkerCount(DateOnly date, int halfHr);
    void UnassignStaff();
    bool IsStaffAlreadyAssigned(DateOnly date, int staffId);
    //todo 生成結果的函示
}