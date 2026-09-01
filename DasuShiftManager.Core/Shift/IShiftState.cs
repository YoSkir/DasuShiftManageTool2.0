using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.Shift;

/// <summary>
/// 定義排班狀態變更與查詢的核心抽象，供遞迴排班流程使用。
/// </summary>
public interface IShiftState
{
    /// <summary>
    /// 將員工排入指定日期與時間段。
    /// </summary>
    /// <param name="date">排班日期。</param>
    /// <param name="staffId">員工識別碼。</param>
    /// <param name="startArrHalfHr">開始的半小時索引。</param>
    /// <param name="workHalfHrs">該班的半小時長度。</param>
    /// <param name="staffType">員工類型，用於判斷是否符合特殊班別限制。</param>
    /// <returns>若新增排班成功則為 <see langword="true"/>，否則為 <see langword="false"/>。</returns>
    bool AssignStaff(DateOnly date, int staffId, int startArrHalfHr, int workHalfHrs, StaffType staffType);

    /// <summary>
    /// 為指定員工標記某一天為休假。
    /// </summary>
    /// <param name="date">休假日期。</param>
    /// <param name="staffId">員工識別碼。</param>
    /// <returns>若標記成功則為 <see langword="true"/>，否則為 <see langword="false"/>。</returns>
    bool AssignStaffDayOff(DateOnly date, int staffId);

    /// <summary>
    /// 取得指定日期與半小時點位已排班的人數。
    /// </summary>
    /// <param name="date">日期。</param>
    /// <param name="arrHalfHr">半小時索引。</param>
    /// <returns>該時段目前已排班的人數。</returns>
    int GetArrHalfHrAssignedStaffCount(DateOnly date, int arrHalfHr);

    /// <summary>
    /// 回滾最新一次的排班操作，供遞迴搜尋回溯狀態使用。
    /// </summary>
    void UnassignStaff();

    /// <summary>
    /// 判斷指定員工在某日是否已經安排班表。
    /// </summary>
    /// <param name="date">要檢查的日期。</param>
    /// <param name="staffId">員工識別碼。</param>
    /// <returns>若該員工已排班則為 <see langword="true"/>。</returns>
    bool IsStaffAlreadyAssigned(DateOnly date, int staffId);

    /// <summary>
    /// 取得指定員工目前連續上班天數。
    /// </summary>
    /// <param name="staffId">員工識別碼。</param>
    /// <returns>連續上班的天數。</returns>
    int GetChainWorkDays(int staffId);

    /// <summary>
    /// 計算員工在指定日期所屬週中已休假的天數。
    /// </summary>
    /// <param name="staffId">員工識別碼。</param>
    /// <param name="date">用來決定週期範圍的日期。</param>
    /// <returns>該週內已排假次數。</returns>
    int GetVacationsOfCurrentWeek(int staffId, DateOnly date);

    int GetWorkHalfHrs(int staffId, DateOnly date, int countDays);
    ShiftInfo GetShiftCopy(int staffId, DateOnly date);
    void AssignShift(Dictionary<int, ShiftInfo> shiftStaffShifts, DateOnly date);
    int GetTotalWorkHalfHrs(int staffId);
    int GetTotalRestDays(int staffId);
}