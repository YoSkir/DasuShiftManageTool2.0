using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool.AssignTool;
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
        assignTool.ShiftDfs(context,context.StartDate,0);
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

    /// <summary>
    /// 建立本月排班使用的狀態模型。
    /// </summary>
    /// <param name="startDate">當月起始日期。</param>
    /// <param name="staffList">員工清單。</param>
    /// <param name="setting">排班設定。</param>
    /// <returns>初始化好的排班狀態實例。</returns>
    public IShiftState GetShiftModel(DateOnly startDate, List<Staff> staffList, Setting setting)
    {
        return new ShiftStateDfs(startDate,setting,staffList);
    }
}