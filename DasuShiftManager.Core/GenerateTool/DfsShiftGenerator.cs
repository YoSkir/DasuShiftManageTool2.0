using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool.AssignTool;
using DasuShiftManager.Core.Shift;

namespace DasuShiftManager.Core.GenerateTool;

public class DfsShiftGenerator : IShiftGenerator
{
    public void StartGenerate(ShiftCreateContext context,IAssignTool assignTool)
    {
        //劃假排入
        AssignDayOff(context);
        //固定班別排入
        AssignFixedShiftStaff(context);
        //遞迴排班
        assignTool.ShiftDfs(context,context.StartDate,0);
        //todo 如果結果為0 嘗試增加虛擬員工再次排班
    }

    private void AssignDayOff(ShiftCreateContext context)
    {
        foreach (var dayOffData in from pair in context.VacationData
                 let date=pair.Key
                 from id in pair.Value
                 select new {date,id})
        {
            context.ShiftState.AssignStaffDayOff(dayOffData.date,dayOffData.id);
        }
    }

    private void AssignFixedShiftStaff(ShiftCreateContext context)
    {
        var date = context.StartDate;
        while (date >= context.StartDate.AddMonths(1))
        {
            var weekday = (int)date.DayOfWeek;
            foreach (var fixedPair in context.Setting.FixedShiftStaff)
            {
                var shift = fixedPair.Value?[weekday];
                if(shift==null||shift.DayOff) continue;
                //跳過排假
                if (context.ShiftState.IsStaffAlreadyAssigned(date, fixedPair.Key)) continue;
                if(!context.ShiftState.AssignStaff(date, fixedPair.Key, shift.StartHalfHr, shift.WorkHalfHrs, StaffType.Normal))
                    throw new InvalidOperationException($"Fixed shift assignment failed, staff id: {fixedPair.Key}");
            }
            date.AddDays(1);
        }
    }

    public IShiftState GetShiftModel(DateOnly startDate, List<Staff> staffList, Setting setting)
    {
        return new ShiftStateDfs(startDate,setting,staffList);
    }
}