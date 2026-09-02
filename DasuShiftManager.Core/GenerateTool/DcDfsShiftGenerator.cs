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
        var tryCount = 0;
        while (!DcDfsTool.AssignMonthly(context))
        {
            tryCount++;
            if (tryCount > 1000)
            {
                Console.WriteLine("嘗試失敗");
                throw new Exception();
            }
        }
        Console.WriteLine($"TryCount: {tryCount}");
    }
}

public static class DcDfsTool
{
    public static bool AssignMonthly(ShiftCreateContext context)
    {
        context.EndDate = context.StartDate.AddDays(27);
        context.ShiftState = new DfsShiftState(context.StartDate, context.EndDate, context.Setting, context.StaffList);
        var date = context.StartDate;

        //帶入上一份班表的連續上班、當周休假 以方便首日排班判斷
        if (context.PrevShiftState != null)
        {
        }
        
        while (date <= context.EndDate)
        {
            var todayAvailableShift = new List<DailyShift>();
            var intDayOfWeek = (int)date.DayOfWeek;
            //指定篩選:
            HashSet<int> dayOffStaff = [.. context.VacationData.TryGetValue(date, out var list) ? list : []];
            HashSet<int> ptoStaff=[.. context.PtoData.TryGetValue(date,out var ptoList)? ptoList : []];
            foreach (var dailyShift in context.DailyShift)
            {
                var skip = false;
                foreach (var staffId in dailyShift.StaffShifts.Keys)
                {
                    var staffShift = dailyShift.StaffShifts[staffId];
                    //篩選劃假
                    if (dayOffStaff.Contains(staffId) && !staffShift.DayOff)
                    {
                        skip = true;
                        break;
                    }
                    //特休
                    if (ptoStaff.Contains(staffId) && !staffShift.DayOff)
                    {
                        skip = true;
                        break;
                    }
                    //篩選指定班
                    if (context.FixedShiftStaff.TryGetValue(staffId, out var fixedShift))
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
                    if (context.PrevShiftState != null || date.DayNumber-context.StartDate.DayNumber>=5)
                    {
                        var satAndDayOffIs0 = date.DayOfWeek == DayOfWeek.Saturday
                                              && context.ShiftState.GetVacationsOfCurrentWeek(staffId, date) == 0;
                        var sunAndDayOffIs1 = date.DayOfWeek == DayOfWeek.Sunday
                                              && context.ShiftState.GetVacationsOfCurrentWeek(staffId, date) == 1;
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
            if (todayAvailableShift.Count == 0) return false;
            //額外篩選:
            var priorityShift = new List<DailyShift>();
            var temp = new List<DailyShift>();
            //偏好排班
            foreach (var dailyShift in todayAvailableShift)
            {
                var skip = false;
                foreach (var staffId in context.PreferShift.Keys)
                {
                    var preferShift = context.PreferShift[staffId];
                    if (dailyShift.StaffShifts[staffId].DayOff)
                        continue;
                    if (dailyShift.StaffShifts[staffId].StartArrHalfHr == preferShift.StartArrHalfHr)
                        continue;
                    skip = true;
                    break;
                }

                if (skip) continue;
                temp.Add(dailyShift);
            }

            priorityShift.AddRange(temp);
            temp.Clear();

            //目前班表時數最低者 除非以符合最低時數 否則優先找排班時長較長 
            if (priorityShift.Count > 1)
            {
                var targetId = -1;
                var minWorkHr = int.MaxValue;
                foreach (var staff in context.StaffList)
                {
                    var workHr = context.ShiftState.GetTotalWorkHalfHrs(staff.Id);
                    if (workHr < minWorkHr)
                    {
                        targetId = staff.Id;
                        minWorkHr = workHr;
                    }
                }

                //不足最低時數才需篩選
                if (minWorkHr < context.Setting.MinMonthWorkHrs*2)
                {
                    //todo 目前先用全班 未來可加入計算往後剩餘上班日去導出最低可排時數
                    var maxShiftHalfHr = context.Setting.ShiftHalfHrType.Max();
                    foreach (var dailyShift in priorityShift)
                    {
                        if (!dailyShift.StaffShifts[targetId].DayOff &&
                            dailyShift.StaffShifts[targetId].WorkHalfHrs == maxShiftHalfHr)
                        {
                            temp.Add(dailyShift);
                        }
                    }

                    if (temp.Count > 0)
                    {
                        priorityShift.Clear();
                        priorityShift.AddRange(temp);
                    }

                    temp.Clear();
                }
            }
            
            //排除連上四天
            if (priorityShift.Count > 1)
            {
                foreach (var dailyShift in priorityShift)
                {
                    var skip = false;
                    foreach (var staff in context.StaffList)
                    {
                        if (context.ShiftState.GetChainWorkDays(staff.Id) == 4 &&
                            !dailyShift.StaffShifts[staff.Id].DayOff)
                        {
                            skip = true;
                            break;
                        }
                    }
                    if(skip) continue;
                    temp.Add(dailyShift);
                }

                if (temp.Count > 0)
                {
                    priorityShift.Clear();
                    priorityShift.AddRange(temp);
                }

                temp.Clear();
            }
            
            //找最少員工連續全班
            if (priorityShift.Count > 1)
            {
                if (temp.Count > 0)
                {
                    priorityShift.Clear();
                    priorityShift.AddRange(temp);
                }

                temp.Clear();
            }

            //結果中隨機排班
            var shift = priorityShift.Count == 0
                ? _getRandomDailyShift(todayAvailableShift)
                : _getRandomDailyShift(priorityShift);
            context.ShiftState.AssignShift(shift.StaffShifts, date);
            //特休捕時數
            foreach (var staffId in ptoStaff)
            {
                context.ShiftState.AssignPto(staffId);
            }
            date = date.AddDays(1);
        }

        //篩選每月時數與休假日
        foreach (var staff in context.StaffList)
        {
            if(context.ShiftState.GetTotalWorkHalfHrs(staff.Id)<context.Setting.MinMonthWorkHrs*2)
                return false;
            if(context.ShiftState.GetTotalRestDays(staff.Id)<context.Setting.MinMonthRestDays)
                return false;
        }
        return true;
    }

    private static DailyShift _getRandomDailyShift(List<DailyShift> results)
    {
        return results[Random.Shared.Next(0, results.Count)];
    }
}