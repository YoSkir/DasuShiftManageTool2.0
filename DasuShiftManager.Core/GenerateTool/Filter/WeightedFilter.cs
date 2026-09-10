using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.GenerateTool.Filter;

public class WeightedFilter:IShiftFilter
{
    public List<DailyShift> Filter(List<DailyShift> shifts, ShiftCreateContext context)
    {
        List<Staff> workHrRankTemp = [.. context.StaffList
            .Where(s => context.ShiftState.GetTotalWorkHalfHrs(s.Id) < context.Setting.MinMonthWorkHrs * 2)
            .OrderBy(s => context.ShiftState.GetTotalWorkHalfHrs(s.Id))];
        var workHrRank = new List<int>?[3];
        var minWorkHr = int.MinValue;
        var rank = -1;
        foreach (var staff in workHrRankTemp)
        {
            var workHr = context.ShiftState.GetTotalWorkHalfHrs(staff.Id);
            if (workHr > minWorkHr)
            {
                rank++;
                if(rank>=workHrRank.Length) break;
                minWorkHr = workHr;
                workHrRank[rank] = [staff.Id];
            }else if (workHr == minWorkHr)
                workHrRank[rank].Add(staff.Id);
        }
        if (workHrRank[0].Count == context.StaffList.Count) workHrRank[0] = null;

        
        List<Staff> earlyShiftRankTemp = [..context.StaffList
            .OrderBy(s=>context.ShiftType.Early[s.Id])];
        var earlyShiftRank = new List<int>?[3];
        var minEarlyShift=int.MinValue;
        rank = -1;
        foreach (var staff in earlyShiftRankTemp)
        {
            var earlyShift = context.ShiftType.Early[staff.Id];
            if (earlyShift > minEarlyShift)
            {
                rank++;
                if(rank>=earlyShiftRank.Length) break;
                minEarlyShift = earlyShift;
                earlyShiftRank[rank] = [staff.Id];
            }else if(earlyShift==minEarlyShift)
                earlyShiftRank[rank].Add(staff.Id);
        }
        if (earlyShiftRank[0].Count == context.StaffList.Count) earlyShiftRank[0] = null;
        
        foreach (var dailyShift in shifts)
        {
            var weight = 10;
            //偏好排班 符合加分 不符合減分
            var score = 0;
            foreach (var staffId in context.PreferShift.Keys)
            {
                var preferShift = context.PreferShift[staffId];
                if (dailyShift.StaffShifts[staffId].DayOff)
                    continue;
                if (dailyShift.StaffShifts[staffId].StartArrHalfHr == preferShift.StartArrHalfHr)
                    score+=weight;
                else if(preferShift.StartArrHalfHr!=-1) score-=weight;
                if (preferShift.LongShift)
                {
                    if(dailyShift.StaffShifts[staffId].WorkHalfHrs >21)
                        score+=weight;
                    else if(preferShift.LongShift) score-=weight;
                }
            }
            dailyShift.WeightedScore+=score;
            dailyShift.WeightCount.Count[WeightType.偏好排班] += score;
            
            //排除已超過最低時數後 依照已排時數排名 排名高者符合班加權較多
            //為避免人數影響加權 可能只排固定幾名
            score = 0;
            rank = 0;
            weight = 1;
            foreach (var staffIdList in workHrRank)
            {
                if(staffIdList==null) break;
                foreach (var staffId in staffIdList)
                {
                    if (dailyShift.StaffShifts[staffId].DayOff)
                    {
                        score-=Math.Max(0,weight-rank);
                        continue;
                    }
                    if(dailyShift.StaffShifts[staffId].WorkHalfHrs >21)
                        score+=Math.Max(0,weight-rank);
                    else score+=Math.Max(0,weight-rank-2);
                }
                rank++;
            }
            dailyShift.WeightedScore+=score;
            dailyShift.WeightCount.Count[WeightType.最低工時] += score;
            
            //連上天數越多 放假時加權越多
            score = 0;
            weight = 10;
            foreach (var staff in context.StaffList)
            {
                var chainDays=context.ShiftState.GetChainWorkDays(staff.Id);
                if (!dailyShift.StaffShifts[staff.Id].DayOff)
                {
                    score -= chainDays+weight-5;
                    continue;
                }
                score += chainDays+weight;
            }
            dailyShift.WeightedScore+=score;
            dailyShift.WeightCount.Count[WeightType.連上天數] = score;
            //早班依舊以排名加權
            rank = 0;
            score = 0;
            weight = 10;
            foreach (var staffIdList in earlyShiftRank)
            {
                if(staffIdList==null)break;
                foreach (var staffId in staffIdList.Where(s=>!dailyShift.StaffShifts[s].DayOff))
                {
                    if(dailyShift.StaffShifts[staffId].Type ==Shared.ShiftType.Early)
                        score+=Math.Max(0,weight-rank);
                    else score-=Math.Max(0,weight-rank);
                }
                rank++;
            }
            dailyShift.WeightedScore += score;
            dailyShift.WeightCount.Count[WeightType.早班平均] = score;
        }
        //取加權分數最高的集合
        var topScore = shifts.Max(s => s.WeightedScore);
        return [.. shifts.Where(s=>s.WeightedScore==topScore)];
    }
}