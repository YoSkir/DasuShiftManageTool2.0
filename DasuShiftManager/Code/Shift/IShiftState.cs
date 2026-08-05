using DasuShiftManager.Code.Entities;

namespace DasuShiftManager.Code.Shift;

public interface IShiftState
{
    bool AssignStaff(DateOnly date, int staffId, int startArrHalfHr, int workHalfHrs, StaffType staffType);
    bool AssignStaffDayOff(DateOnly date, int staffId);
    int GetWorkerCount(DateOnly date, int arrHalfHr);
    void UnassignStaff();
    bool IsStaffAlreadyAssigned(DateOnly date, int staffId);
    //todo 生成結果的函示
}