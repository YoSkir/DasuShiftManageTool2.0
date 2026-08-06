using DasuShiftManager.Code.Entities;

namespace DasuShiftManager.Code.Shift;

public interface IShiftState
{
    /**
     * 排入員工
     */
    bool AssignStaff(DateOnly date, int staffId, int startArrHalfHr, int workHalfHrs, StaffType staffType);
    /**
     * 員工排假
     */
    bool AssignStaffDayOff(DateOnly date, int staffId);
    /**
     * 獲得半時陣列指定日期與陣列半時目前已排員工數量
     */
    int GetArrHalfHrAssignedStaffCount(DateOnly date, int arrHalfHr);
    /**
     * 取消上一步排班 用於遞迴的狀態回滾
     */
    void UnassignStaff();
    /**
     * 檢查當日特定員工是否已排 用於篩選可排員工
     */
    bool IsStaffAlreadyAssigned(DateOnly date, int staffId);
    /**
     * 獲得遞迴時員工連續上班天數 用於篩選可排員工
     */
    int GetChainWorkDays(int staffId);
    /**
     * 計算當周員工已排假數 用於判斷是否符合每周最低假日數
     */
    int GetVacationsOfCurrentWeek(int staffId, DateOnly date);
    //todo 生成結果的函示
}