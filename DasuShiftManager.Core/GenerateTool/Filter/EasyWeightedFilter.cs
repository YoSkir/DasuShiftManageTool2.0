using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.GenerateTool.Filter;

public class EasyWeightedFilter : IShiftFilter
{
    public List<DailyShift> Filter(List<DailyShift> shifts, ShiftCreateContext context)
    {
        HashSet<int> maxShiftHalfHr = [22, 26];
        var minWorkHrStaffIds = new List<int>();
        var minWorkHr = int.MaxValue;
        
        //用於需無視固定班員工的列表
        var noFixedStaffList=context.StaffList.Where(s=>!context.FixedShiftStaff.ContainsKey(s.Id)).ToList();
        
        foreach (var staff in noFixedStaffList
                     .Where(s => context.ShiftState.GetTotalWorkHalfHrs(s.Id) < context.Setting.MinMonthWorkHrs * 2))
        {
            var workHr = context.ShiftState.GetTotalWorkHalfHrs(staff.Id);
            if (workHr < minWorkHr)
            {
                minWorkHrStaffIds.Clear();
                minWorkHrStaffIds.Add(staff.Id);
                minWorkHr = workHr;
            }else if (workHr == minWorkHr) minWorkHrStaffIds.Add(staff.Id);
        }
        var minEarlyShiftStaffIds = new List<int>();
        var minEarlyShiftCount=int.MaxValue;
        foreach (var staff in noFixedStaffList)
        {
            var earlyShift=context.ShiftType.Early[staff.Id];
            if (earlyShift < minEarlyShiftCount)
            {
                minEarlyShiftStaffIds.Clear();
                minEarlyShiftStaffIds.Add(staff.Id);
                minEarlyShiftCount = earlyShift;
            }else if(minEarlyShiftCount==earlyShift) minEarlyShiftStaffIds.Add(staff.Id);
        }
        //偏好排班
        foreach (var dailyShift in shifts)
        {
            var pass = true;
            foreach (var staffId in context.PreferShift.Keys)
            {
                var preferShift = context.PreferShift[staffId];
                if (dailyShift.StaffShifts[staffId].DayOff)
                    continue;
                if (dailyShift.StaffShifts[staffId].StartArrHalfHr == preferShift.StartArrHalfHr)
                    continue;
                pass=false;
                break;
            }
            if(pass) dailyShift.AddWeightScore(WeightType.偏好排班,2);
            
            //目前班表時數最低者 除非以符合最低時數 否則優先找排班時長較長 
            if(minWorkHrStaffIds
                   .Where(i => !dailyShift.StaffShifts[i].DayOff)
                   .Where(i => maxShiftHalfHr.Contains(dailyShift.StaffShifts[i].WorkHalfHrs)).ToList().Count>0)
                dailyShift.AddWeightScore(WeightType.最低工時,2);
            //排除連上四天
            if (noFixedStaffList
                    .Where(s => context.ShiftState.GetChainWorkDays(s.Id) > 2)
                    .Where(s => dailyShift.StaffShifts[s.Id].DayOff).ToList().Count > 0)
                dailyShift.AddWeightScore(WeightType.連上天數,2);
            //早班平均
            if(minEarlyShiftStaffIds.Where(s=>dailyShift.StaffShifts[s].Type==Shared.ShiftType.Early).ToList().Count>0)
                dailyShift.AddWeightScore(WeightType.早班平均,2);
        }
        var topScore=shifts.Max(s=>s.WeightedScore);
        return [.. shifts.Where(s => s.WeightedScore == topScore)];
    }
}