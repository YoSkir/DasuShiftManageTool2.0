using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.GenerateTool.ResultSaver;

public class DcDfsResultSaver:IResultSaver
{
    public void SaveResult(ShiftCreateContext context)
    {
        context.IdCount++;
        var result = new ShiftResult(){ShiftId =  context.IdCount};
        foreach (var staff in context.StaffList)
        {
            result.StaffShifts[staff.Id]=context.GetShiftCopy(staff.Id,context.StartDate);
        }
        context.DailyShift.Add(result);
    }
}