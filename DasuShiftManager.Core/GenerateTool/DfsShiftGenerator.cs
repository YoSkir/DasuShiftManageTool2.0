using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool.AssignTool;
using DasuShiftManager.Core.GenerateTool.ResultSaver;
using DasuShiftManager.Core.Shift;

namespace DasuShiftManager.Core.GenerateTool;

/// <summary>
/// 以深度優先搜尋方式執行排班生成，先處理休假與固定班別，再進入遞迴分配。
/// </summary>
public class DfsShiftGenerator : IShiftGenerator
{
    /// <summary>
    /// 啟動排班流程，依序套用休假、固定班別與搜尋分配。
    /// </summary>
    /// <param name="context">排班上下文。</param>
    /// <param name="assignTool">具體的排班分配工具。</param>
    public void StartGenerate(ShiftCreateContext context,IAssignTool assignTool)
    {
        //劃假排入
        AssignDayOff(context);
        //固定班別排入
        AssignFixedShiftStaff(context);
        //遞迴排班
        context.ResultSaver = new MultipleRankResultSaver();
        context.EndDate = context.StartDate.AddMonths(1).AddDays(-1);
        context.PruningStatement = new PruningStatement()
        {
            MaxDayyOff = 4,
            MaxWorkHalfHrGap = 52
        };
        context.ShiftState = new ShiftStateDfs(context.StartDate,context.EndDate,context.Setting,context.StaffList);
        assignTool.ShiftDfs(context,context.StartDate,context.NextUndoneArrHalfHr(context.StartDate));
        //todo 如果結果為0 嘗試增加虛擬員工再次排班
    }

    /// <summary>
    /// 將休假資料寫入當前排班狀態。
    /// </summary>
    /// <param name="context">目前排班上下文。</param>
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

    /// <summary>
    /// 將固定班別員工先排入當月班表。
    /// </summary>
    /// <param name="context">目前排班上下文。</param>
    private void AssignFixedShiftStaff(ShiftCreateContext context)
    {
        var date = context.StartDate;
        while (date <=context.EndDate)
        {
            var weekday = (int)date.DayOfWeek;
            foreach (var fixedPair in context.Setting.FixedShiftStaff)
            {
                var shift = fixedPair.Value?[weekday];
                if(shift==null||shift.DayOff) continue;
                //跳過排假
                if (context.ShiftState.IsStaffAlreadyAssigned(date, fixedPair.Key)) continue;
                if(!context.ShiftState.AssignStaff(date, fixedPair.Key, shift.StartArrHalfHr, shift.WorkHalfHrs, context.GetStaffType(fixedPair.Key)))
                    throw new InvalidOperationException($"Fixed shift assignment failed, staff id: {fixedPair.Key}");
            }
            date=date.AddDays(1);
        }
    }
    
}