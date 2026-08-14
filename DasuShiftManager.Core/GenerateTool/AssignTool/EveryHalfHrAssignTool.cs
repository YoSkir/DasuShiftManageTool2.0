namespace DasuShiftManager.Core.GenerateTool.AssignTool;

/**
 * 每個半時都嘗試排的排法
 */
public class EveryHalfHrAssignTool : IAssignTool
{
    public void ShiftDfs(ShiftCreateContext context, DateOnly date, int arrHalfHr)
    {
        //存結果條件
        if (date >= context.StartDate.AddMonths(1))
        {
            context.ResultSaver.SaveResult(context);
            return;
        }
        if (arrHalfHr+1 >= context.Setting.ShiftHalfHrCount)
            ShiftDfs(context, date.AddDays(1), 0);
        //嘗試排班
        //todo 測試一下 目前排法是所有半時都嘗試 如果太耗效能，要改成先排最低需求，再依不夠工時另跑補工時遞迴
        foreach (var ss in from staff in context.GetAvailableStaffs(date)
                 from shiftHalfHr in context.Setting.ShiftHalfHrType
                 where shiftHalfHr<=context.Setting.ShiftHalfHrCount-arrHalfHr
                 select new {staff,shiftHalfHr})
        {
            if(!context.ShiftState.AssignStaff(date,ss.staff.Id,arrHalfHr,ss.shiftHalfHr,ss.staff.StaffType))
                continue;
            if(context.IsWorkerEnough(date, arrHalfHr))
                ShiftDfs(context, date, arrHalfHr+1);
            ShiftDfs(context, date, arrHalfHr);
            context.ShiftState.UnassignStaff();
        }
        //這裡不順便補上沒排班人員的假日 是因為會擾亂遞迴歷史紀錄
    }
}