using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool.AssignTool;
using DasuShiftManager.Core.GenerateTool.ResultSaver;
using DasuShiftManager.Core.Shift;

namespace DasuShiftManager.Core.GenerateTool;

public class DcDfsShiftGenerator : IShiftGenerator
{
    public void StartGenerate(ShiftCreateContext context, IAssignTool assignTool)
    {
        //分治法 排出一天所有可能後儲存
        context.ResultSaver = new DcDfsResultSaver();
        context.EndDate = context.StartDate.AddDays(1);
        context.ShiftState = new ShiftStateDfs(context.StartDate, context.EndDate, context.Setting, context.StaffList);
        assignTool.ShiftDfs(context, context.StartDate, context.NextUndoneArrHalfHr(context.StartDate));
        //每日班表組合
        //每天篩選: 符合劃假、符合指定班、符合最高連上天數、週日時檢查符合最低放假天數、最後符合最低時數、指定早或晚、藥師需求
        //額外篩選(如果有就套用 沒有就不套): 不要連續全班 不要晚接早 不要連續放超過3天 偏好排班
        //多結果時使用亂數(或是之前的排行法找當下複數最優解)
    }
}