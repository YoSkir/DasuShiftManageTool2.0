using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.GenerateTool.Filter;

public class OrderFilter : IShiftFilter
{
    public List<DailyShift> Filter(List<DailyShift> shifts, ShiftCreateContext context)
    {
        //額外篩選:
        var priorityShift = new List<DailyShift>(shifts);
        var temp = new List<DailyShift>();
        //偏好排班
        foreach (var dailyShift in priorityShift)
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
        if (temp.Count > 0)
        {
            priorityShift.Clear();
            priorityShift.AddRange(temp);
        }
        temp.Clear();

        
        //目前班表時數最低者 除非以符合最低時數 否則優先找排班時長較長 
        var targetId = -1;
        var minWorkHr = int.MaxValue;
        foreach (var staff in context.StaffList.Where(s=>context.ShiftState.GetTotalWorkHalfHrs(s.Id)<context.Setting.MinMonthWorkHrs*2))
        {
            var workHr = context.ShiftState.GetTotalWorkHalfHrs(staff.Id);
            if (workHr < minWorkHr)
            {
                targetId = staff.Id;
                minWorkHr = workHr;
            }
        }
        //todo 目前先用全班 未來可加入計算往後剩餘上班日去導出最低可排時數
        if (targetId > 0)
        {
            HashSet<int> maxShiftHalfHr = [22, 26];
            temp.AddRange(priorityShift
                .Where(d => !d.StaffShifts[targetId].DayOff)
                .Where(d => maxShiftHalfHr.Contains(d.StaffShifts[targetId].WorkHalfHrs)));

            if (temp.Count > 0)
            {
                priorityShift.Clear();
                priorityShift.AddRange(temp);
            }

            temp.Clear();
        }
        
        //排除連上四天
        temp.AddRange(priorityShift
            .Where(d => context.StaffList.Any(s => context.ShiftState.GetChainWorkDays(s.Id) == 3
                                                   && d.StaffShifts[s.Id].DayOff)));
        if (temp.Count > 0)
        {
            priorityShift.Clear();
            priorityShift.AddRange(temp);
        }
        temp.Clear();

        //早班平均
        targetId = -1;
        var minEarlyShiftCount = int.MaxValue;
        foreach (var staffId in context.ShiftType.Early.Keys)
        {
            var earlyShiftCount = context.ShiftType.Early[staffId];
            if (earlyShiftCount < minEarlyShiftCount)
            {
                minEarlyShiftCount = earlyShiftCount;
                targetId = staffId;
            }
        }

        if (targetId > 0)
        {
            temp.AddRange(priorityShift
                .Where(s => s.StaffShifts[targetId].Type == Entities.ShiftType.Early));
            if (temp.Count > 0)
            {
                priorityShift.Clear();
                priorityShift.AddRange(temp);
            }
            temp.Clear();
        }
        
        return priorityShift;
    }
}