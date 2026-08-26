using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool.AssignTool;
using DasuShiftManager.Core.GenerateTool.ResultSaver;
using DasuShiftManager.Core.Shift;

namespace DasuShiftManager.Core.GenerateTool;

public class DcDfsShiftGenerator : IShiftGenerator
{
    public void StartGenerate(ShiftCreateContext context, IAssignTool assignTool)
    {
        //固定班別排入
        AssignFixedShiftStaff(context);
        //分治法 排出乾淨的一周所有可能後儲存
        context.ResultSaver = new DcDfsResultSaver();
        //找到周一的日期
        var startDate = context.StartDate.AddDays(DayOfWeek.Monday - context.StartDate.DayOfWeek);
        context.StartDate=startDate;
        //設定搜尋七天
        context.EndDate = startDate.AddDays(6);
        context.PruningStatement=  new PruningStatement()
        {
            MaxDayyOff = 4,
            MaxWorkHalfHrGap = 52
        };
        //設定遞迴容器
        context.ShiftState = new ShiftStateDfs(startDate, context.EndDate, context.Setting, context.StaffList);
        assignTool.ShiftDfs(context, startDate, context.NextUndoneArrHalfHr(startDate));
    }

    private void AssignFixedShiftStaff(ShiftCreateContext context)
    {
        var date = context.StartDate;
        while (date <= context.EndDate)
        {
            var weekday = (int)date.DayOfWeek;
            foreach (var fixedPair in context.Setting.FixedShiftStaff)
            {
                var shift = fixedPair.Value?[weekday];
                if (shift == null) continue;
                if (shift.DayOff)
                {
                    if (!context.ShiftState.AssignStaffDayOff(date, fixedPair.Key))
                        throw new InvalidOperationException($"Fixed dayoff assignment failed, staff id: {fixedPair.Key}");
                    continue;
                }

                //跳過排假
                if (context.ShiftState.IsStaffAlreadyAssigned(date, fixedPair.Key)) continue;
                if (!context.ShiftState.AssignStaff(date, fixedPair.Key, shift.StartArrHalfHr, shift.WorkHalfHrs,
                        context.GetStaffType(fixedPair.Key)))
                    throw new InvalidOperationException($"Fixed shift assignment failed, staff id: {fixedPair.Key}");
            }

            date = date.AddDays(1);
        }
    }
}