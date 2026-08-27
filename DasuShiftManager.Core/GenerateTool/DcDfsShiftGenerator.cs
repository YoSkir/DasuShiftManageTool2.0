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
        context.EndDate = context.StartDate;
        context.ShiftState = new DfsShiftState(context.StartDate, context.EndDate, context.Setting, context.StaffList);
        assignTool.ShiftDfs(context, context.StartDate, context.NextUndoneArrHalfHr(context.StartDate));
        //每日班表組合
        context.EndDate = context.StartDate.AddMonths(1).AddDays(-1);
        context.ShiftState = new DfsShiftState(context.StartDate, context.EndDate, context.Setting, context.StaffList);
        var date=context.StartDate;
        //帶入上一份班表的連續上班、當周休假 以方便首日排班判斷
        if (context.PrevShiftState != null)
        {
            
        }
        //每天篩選: 最後符合最低時數、指定早或晚、藥師需求
        //額外篩選(如果有就套用 沒有就不套): 不要連續全班 不要晚接早 不要連續放超過3天 偏好排班
        //多結果時使用亂數(或是之前的排行法找當下複數最優解)
        while (date<=context.EndDate)
        {
            var todayAvailableShift=new List<DailyShift>();
            var intDayOfWeek = (int)date.DayOfWeek;
            //指定篩選:
            HashSet<int> dayOffStaff = [.. context.VacationData.TryGetValue(date, out var list) ? list : []];
            foreach (var dailyShift in context.DailyShift)
            {
                var skip = false;
                foreach (var staffId in dailyShift.StaffShifts.Keys)
                {
                    var staffShift=dailyShift.StaffShifts[staffId];
                    //篩選劃假
                    if (dayOffStaff.Contains(staffId) && !staffShift.DayOff)
                    {
                        skip = true;
                        break;
                    }
                    //篩選指定班
                    if(!context.FixedShiftStaff.TryGetValue(staffId,out var fixedShift))
                        continue;
                    var todayFixedShift = fixedShift[intDayOfWeek];
                    if(todayFixedShift==null) continue;
                    if (todayFixedShift.DayOff)
                    {
                        if (!staffShift.DayOff)
                        {
                            skip = true;
                            break;
                        }
                    }
                    else
                    {
                        if (todayFixedShift.StartArrHalfHr != staffShift.StartArrHalfHr
                            || todayFixedShift.WorkHalfHrs != staffShift.WorkHalfHrs)
                        {
                            skip = true;
                            break;
                        }
                    }
                    //篩選最高連上天數
                    
                    //週日篩選最低排假
                }
                if(skip) continue;
                todayAvailableShift.Add(dailyShift);
            }
            //額外篩選:
            //找最少員工連續全班
            //目前班表時數最低者 除非以符合最低時數 否則優先找排班時長較長 
            
            //結果中隨機排班
            var shift = _getRandomDailyShift(todayAvailableShift);
            context.ShiftState.AssignShift(shift.StaffShifts,date);
            date = date.AddDays(1);
        }

    }

    private DailyShift _getRandomDailyShift(List<DailyShift> results)
    {
        return results[Random.Shared.Next(0, results.Count)];
    }
}