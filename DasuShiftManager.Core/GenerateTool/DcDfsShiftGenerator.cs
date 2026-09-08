using System.Globalization;
using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool.AssignTool;
using DasuShiftManager.Core.GenerateTool.Filter;
using DasuShiftManager.Core.GenerateTool.ResultSaver;
// using DasuShiftManager.Core.Log;
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
        Console.WriteLine($"組合數量:{context.DailyShift.Count}");
        //每日班表組合
        var tryCount = 0;
        while (!DcDfsTool.AssignMonthly(context))
        {
            tryCount++;
            if (tryCount > 200)
            {
                Console.WriteLine("嘗試失敗");
                throw new Exception("嘗試失敗");
            }
        }

        Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)} 排班完成! 嘗試次數: {tryCount}");
    }
}

public static class DcDfsTool
{
    public static bool AssignMonthly(ShiftCreateContext context)
    {
        //初始化與統計清0
        context.EndDate = context.StartDate.AddDays(27);
        context.ShiftState = new DfsShiftState(context.StartDate, context.EndDate, context.Setting, context.StaffList);
        context.ClearShiftTypeCount();
        var date = context.StartDate;

        //帶入上一份班表的連續上班、當周休假 以方便首日排班判斷
        if (context.PrevShiftState != null)
        {
            foreach (var staff in context.StaffList)
            {
                context.ShiftState.SetChainWorkDays(staff.Id, context.PrevShiftState.GetChainWorkDays(staff.Id));
                context.ShiftState.SetCurrentWeekDayOff(staff.Id, date,
                    context.PrevShiftState.GetRestDaysOfCurrentWeek(staff.Id, date));
            }
        }
        
        while (date <= context.EndDate)
        {
            var todayAvailableShift = new List<DailyShift>();
            var intDayOfWeek = (int)date.DayOfWeek;
            //指定篩選:
            HashSet<int> dayOffStaff = [.. context.VacationData.TryGetValue(date, out var list) ? list : []];
            HashSet<int> ptoStaff = [.. context.PtoData.TryGetValue(date, out var ptoList) ? ptoList : []];
            foreach (var dailyShift in context.DailyShift)
            {
                //是否符合最低人數
                if (context.WeekHalfHrWorkers[date.DayOfWeek].EveryHalfHrMinWorkers
                    .Where((v, i) => dailyShift.StaffCount[i] < v).Any()) continue;
                
                var skip = false;
                foreach (var staffId in dailyShift.StaffShifts.Keys)
                {
                    var staffShift = dailyShift.StaffShifts[staffId];
                    //用於固定班員工 如果有指定排，就不用看固定班設定
                    var assigned = false;
                    //篩選劃假
                    if (dayOffStaff.Contains(staffId))
                    {
                        assigned = true;
                        if (!staffShift.DayOff)
                        {
                            skip = true;
                            break;
                        }
                    }

                    //特休
                    if (ptoStaff.Contains(staffId))
                    {
                        assigned = true;
                        if (!staffShift.DayOff)
                        {
                            skip = true;
                            break;
                        }
                    }

                    //篩選指定班
                    if (context.AssignedShift.TryGetValue(staffId, out var assignedShift)
                        && assignedShift.TryGetValue(date, out var shiftInfo))
                    {
                        assigned = true;
                        if (staffShift.StartArrHalfHr != shiftInfo.StartArrHalfHr ||
                            staffShift.WorkHalfHrs != shiftInfo.WorkHalfHrs)
                        {
                            skip = true;
                            break;
                        }
                    }

                    //篩選固定班
                    if (!assigned && context.FixedShiftStaff.TryGetValue(staffId, out var fixedShift))
                    {
                        var todayFixedShift = fixedShift[intDayOfWeek];
                        if (todayFixedShift != null)
                        {
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
                        }
                    }

                    //篩選最高連上天數
                    if (context.ShiftState.GetChainWorkDays(staffId) == context.Setting.MaxChainWorkDays &&
                        !staffShift.DayOff)
                    {
                        skip = true;
                        break;
                    }

                    //篩選每周最低排假
                    //須排除無前班表並且排班日還沒5天
                    if (context.PrevShiftState != null || date.DayNumber - context.StartDate.DayNumber >= 5)
                    {
                        var satAndDayOffIs0 = date.DayOfWeek == DayOfWeek.Saturday
                                              && context.ShiftState.GetRestDaysOfCurrentWeek(staffId, date) == 0;
                        var sunAndDayOffIs1 = date.DayOfWeek == DayOfWeek.Sunday
                                              && context.ShiftState.GetRestDaysOfCurrentWeek(staffId, date) == 1;
                        if ((satAndDayOffIs0 || sunAndDayOffIs1)
                            && !staffShift.DayOff)
                        {
                            skip = true;
                            break;
                        }
                    }
                }

                if (skip) continue;
                todayAvailableShift.Add(dailyShift);
            }

            //無符合結果時斷開
            if (todayAvailableShift.Count == 0)
            {
                return false;
            }

            var priorityShift = context.Filter.Filter(todayAvailableShift, context);

            //結果中隨機排班
            var shift = priorityShift.Count == 0
                ? _getRandomDailyShift(todayAvailableShift)
                : _getRandomDailyShift(priorityShift);
            context.AssignShift(shift.StaffShifts, date);
            //特休捕時數
            foreach (var staffId in ptoStaff)
            {
                context.ShiftState.AssignPto(staffId);
            }

            date = date.AddDays(1);
        }

        //找排班需求沒排滿的天數
        var editableShiftDate = new List<AssignableHrInfo>();
        date = context.StartDate;
        while (date <= context.EndDate)
        {
            var info = context.GetAssignableHrInfo(date);
            if (info.UndoneHalfHrCount>0)
                editableShiftDate.Add(info);
            date = date.AddDays(1);
        }

        //篩選每月時數與休假日
        foreach (var staff in context.StaffList)
        {
            var totalWorkHrs = context.ShiftState.GetTotalWorkHalfHrs(staff.Id);
            if (totalWorkHrs >= context.Setting.MinMonthWorkHrs * 2) continue;
            //找到能動用的天
            List<AssignableHrInfo> staffEditableDate =
            [
                .. editableShiftDate
                    .Where(d => !context.VacationData.TryGetValue(d.Date, out var vacationList)
                                || vacationList.Contains(staff.Id))
                    .Where(d => !context.PtoData.TryGetValue(d.Date, out var ptoList)
                                || ptoList.Contains(staff.Id))
                    .Where(d => !context.AssignedShift.TryGetValue(staff.Id, out var assigned)
                                || assigned.ContainsKey(d.Date))
                    .Where(d => d.UndoneHalfHrCount > 0)
            ];
            //嘗試把多餘排假排班
            var totalRestDays = context.ShiftState.GetTotalRestDays(staff.Id);
            if (totalRestDays>context.Setting.MinMonthRestDays)
            {
                staffEditableDate =
                    [.. staffEditableDate.Where(d => d.UndoneHalfHrCount >= context.Setting.ShiftHalfHrType.Min())];
                //排班後會連上4天的候選
                var fourDaysCandidate = new List<AssignableHrInfo>();
                foreach (var undoneHrInfo in staffEditableDate)
                {
                    var shiftInfo = context.ShiftState.GetShiftCopy(staff.Id, undoneHrInfo.Date);
                    if (!shiftInfo.DayOff ||
                        context.ShiftState.GetRestDaysOfCurrentWeek(staff.Id, undoneHrInfo.Date) < 3)
                        continue;
                    //檢查把排假拿掉後會連上幾天 如低於4天則直接排
                    var continueWorkDay = 1;
                    var searchDate = undoneHrInfo.Date.AddDays(-1);
                    while (searchDate >= context.StartDate)
                    {
                        if (context.ShiftState.GetShiftCopy(staff.Id, searchDate).DayOff)
                            break;
                        continueWorkDay++;
                        searchDate = searchDate.AddDays(-1);
                    }

                    searchDate = undoneHrInfo.Date.AddDays(1);
                    while (searchDate <= context.EndDate)
                    {
                        if (context.ShiftState.GetShiftCopy(staff.Id, searchDate).DayOff)
                            break;
                        continueWorkDay++;
                        searchDate = searchDate.AddDays(1);
                    }

                    if (continueWorkDay < 4)
                    {
                        context.ShiftState.UnassignStaff(undoneHrInfo.Date, staff.Id);
                        context.AssignStaff(staff.Id, undoneHrInfo.Date,
                            new ShiftInfo(undoneHrInfo.UndoneArrHalfHr, undoneHrInfo.UndoneHalfHrCount));
                        totalWorkHrs += undoneHrInfo.UndoneHalfHrCount;
                        context.RefreshUndoneInfo(undoneHrInfo);
                        totalRestDays--;
                        if (totalWorkHrs >= context.Setting.MinMonthWorkHrs * 2||totalRestDays<=context.Setting.MinMonthRestDays) break;
                    }
                    else if (continueWorkDay == 4) fourDaysCandidate.Add(undoneHrInfo);
                }
                if(totalWorkHrs >= context.Setting.MinMonthWorkHrs * 2) continue;
                if (totalRestDays>context.Setting.MinMonthRestDays)
                {
                    foreach (var undoneHrInfo in fourDaysCandidate
                                 .Where(info=>context.ShiftState.GetRestDaysOfCurrentWeek(staff.Id, info.Date) < 3))
                    {
                        context.ShiftState.UnassignStaff(undoneHrInfo.Date, staff.Id);
                        context.AssignStaff(staff.Id, undoneHrInfo.Date,
                            new ShiftInfo(undoneHrInfo.UndoneArrHalfHr, undoneHrInfo.UndoneHalfHrCount));
                        totalWorkHrs += undoneHrInfo.UndoneHalfHrCount;
                        context.RefreshUndoneInfo(undoneHrInfo);
                        totalRestDays--;
                        if (totalWorkHrs >= context.Setting.MinMonthWorkHrs * 2||totalRestDays<=context.Setting.MinMonthRestDays) break;
                    }
                }
            }
            if (totalWorkHrs >= context.Setting.MinMonthWorkHrs * 2) continue;
            
            //加長不變更班別 延長6小班或10小班
            foreach (var undoneHrInfo in staffEditableDate)
            {
                var shiftInfo = context.ShiftState.GetShiftCopy(staff.Id, undoneHrInfo.Date);
                if (shiftInfo.WorkHalfHrs is not (13 or 22))
                    continue;
                if (shiftInfo.StartArrHalfHr + shiftInfo.WorkHalfHrs == undoneHrInfo.UndoneArrHalfHr
                    && undoneHrInfo.UndoneHalfHrCount >= 4)
                {
                    context.ShiftState.UnassignStaff(undoneHrInfo.Date, staff.Id);
                    if (!context.ShiftState.AssignStaff(undoneHrInfo.Date, staff.Id, shiftInfo.StartArrHalfHr,
                            shiftInfo.WorkHalfHrs + 4, staff.StaffType))
                        throw new InvalidOperationException(
                            $"Error while fixing short shift for {staff.Name} on {undoneHrInfo.Date.ToShortDateString()}");
                    totalWorkHrs += 4;
                    context.RefreshUndoneInfo(undoneHrInfo);
                }
                if (totalWorkHrs >= context.Setting.MinMonthWorkHrs * 2) break;
            }
            if(totalWorkHrs >= context.Setting.MinMonthWorkHrs * 2 ) continue;
            
            //嘗試把短班換成長班
            // staffEditableDate = [..staffEditableDate.Where(info=>info.UndoneHalfHrCount>0)];
            // foreach (var undoneHrInfo in staffEditableDate)
            // {
            //     var shiftInfo = context.ShiftState.GetShiftCopy(staff.Id, undoneHrInfo.Date);
            //     if(shiftInfo.DayOff) continue;
            //     
            // }
            
            if (totalWorkHrs < context.Setting.MinMonthWorkHrs * 2) return false;
        }

        foreach (var staff in context.StaffList)
        {
            var totalRestDays = context.ShiftState.GetTotalRestDays(staff.Id);

            if (totalRestDays < context.Setting.MinMonthRestDays)
            {
                //計算目前已排班表 如果能達到休假日並且不影響班表就修改
                return false;
            }
        }
        return true;
    }

    private static DailyShift _getRandomDailyShift(List<DailyShift> results)
    {
        return results[Random.Shared.Next(0, results.Count)];
    }
}