using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.GenerateTool.ResultSaver;

public class DcDfsResultSaver:IResultSaver
{
    public void SaveResult(ShiftCreateContext context)
    {
        context.IdCount++;
        var result = new DailyShift(){ShiftId =  context.IdCount,StaffCount = new int[context.Setting.ShiftHalfHrCount]};
        foreach (var staff in context.StaffList)
        {
            result.StaffShifts[staff.Id]=context.GetShiftCopy(staff.Id,context.StartDate);
        }

        for (var i = 0; i < context.Setting.ShiftHalfHrCount; i++)
        {
            result.StaffCount[i] = context.ShiftState.GetArrHalfHrAssignedStaffCount(context.StartDate, i);
        }
        context.DailyShift.Add(result);
    }
}