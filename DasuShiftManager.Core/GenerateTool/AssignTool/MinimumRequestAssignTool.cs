namespace DasuShiftManager.Core.GenerateTool.AssignTool;

/**
 * 只排最低限度需求員工的排法
 */
public class MinimumRequestAssignTool:IAssignTool
{
    public void ShiftDfs(ShiftCreateContext context, DateOnly date, int arrHalfHr)
    {
        //存結果條件
        if (date >= context.StartDate.AddMonths(1))
        {
            context.ResultSaver.SaveResult(context);
            return;
        }
        //時間推進條件
        if (context.IsWorkerEnough(date, arrHalfHr))
        {
            arrHalfHr++;
            if (arrHalfHr >= context.Setting.ShiftHalfHrCount)
            {
                arrHalfHr = 0;
                date.AddDays(1);
                //這裡不順便補上沒排班人員的假日 是因為會擾亂遞迴歷史紀錄
            }
            ShiftDfs(context, date, arrHalfHr);
            return;
        }
        //嘗試排班
        foreach (var ss in from staff in context.GetAvailableStaffs(date)
                 from shiftHalfHr in context.Setting.ShiftHalfHrType
                 where shiftHalfHr<=context.Setting.ShiftHalfHrCount-arrHalfHr
                 select new {staff,shiftHalfHr})
        {
            if(!context.ShiftState.AssignStaff(date,ss.staff.Id,arrHalfHr,ss.shiftHalfHr,ss.staff.StaffType))
                continue;
            ShiftDfs(context, date, arrHalfHr);
            context.ShiftState.UnassignStaff();
        }
    }
}