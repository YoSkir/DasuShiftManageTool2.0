namespace DasuShiftManager.Core.GenerateTool.AssignTool;

/**
 * 每個半時都嘗試排的排法
 */
/// <summary>
/// 以「每個半小時都嘗試排班」的方式執行深度優先搜尋。
/// </summary>
public class EveryPossibleAssignTool : IAssignTool
{
    /// <summary>
    /// 遞迴嘗試將員工安插到指定日期與半小時點，直到完成本月排班或找到候選結果。
    /// </summary>
    /// <param name="context">目前排班上下文。</param>
    /// <param name="date">當前處理日期。</param>
    /// <param name="arrHalfHr">當前處理的半小時索引。</param>
    public void ShiftDfs(ShiftCreateContext context, DateOnly date, int arrHalfHr)
    {
        //存結果條件
        if (date > context.EndDate)
        {
            context.ResultSaver.SaveResult(context);
            return;
        }

        if (arrHalfHr >= context.Setting.ShiftHalfHrCount)
        {
            ShiftDfs(context, date.AddDays(1), context.NextUndoneArrHalfHr(date,0));
            return;
        }
        //嘗試排班
        //todo 測試一下 目前排法是所有半時都嘗試 如果太耗效能，要改成先排最低需求，再依不夠工時另跑補工時遞迴
        foreach (var ss in from staff in context.GetAvailableStaffs(date)
                 from shiftHalfHr in context.Setting.ShiftHalfHrType
                 where shiftHalfHr<=context.Setting.ShiftHalfHrCount-arrHalfHr
                 select new {staff,shiftHalfHr})
        {
            if(!context.ShiftState.AssignStaff(date,ss.staff.Id,arrHalfHr,ss.shiftHalfHr,ss.staff.StaffType))
                continue;
            ShiftDfs(context, date, context.NextUndoneArrHalfHr(date,arrHalfHr));
            context.ShiftState.UnassignStaff();
        }
        //這裡不順便補上沒排班人員的假日 是因為會擾亂遞迴歷史紀錄
    }
}