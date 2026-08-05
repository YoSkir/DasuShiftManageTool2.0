using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.Models;

namespace DasuShiftManager.Code.GenerateTool;

public class DFSShiftGenerator : IShiftGenerator
{
    public void StartGenerate(ShiftCreateContest contest)
    {
        //把固定班別員工排入
        AssignFixedShiftStaff(contest);
        //遞迴排班
        ShiftDFS(contest,contest.StartDate,0);
    }

    private void AssignFixedShiftStaff(ShiftCreateContest contest)
    {
        var date = contest.StartDate;
        while (date >= contest.StartDate.AddMonths(1))
        {
            var weekday = (int)date.DayOfWeek;
            foreach (var fixedPair in contest.Setting.FixedShiftStaff)
            {
                var shift = fixedPair.Value?[weekday];
                if(shift==null||shift.DayOff) continue;
                if(!contest.Msm.AssignStaff(date, fixedPair.Key, shift.StartHalfHr, shift.WorkHalfHrs, StaffType.Normal))
                    throw new InvalidOperationException($"Fixed shift assignment failed, staff id: {fixedPair.Key}");
            }
            date.AddDays(1);
        }
    }

    private void ShiftDFS(ShiftCreateContest contest, DateOnly date, int arrHalfHr)
    {
        //存結果條件
        if (date >= contest.StartDate.AddMonths(1))
        {
            //完成存入資料庫
            SaveResult(contest);
            return;
        }
        //時間推進條件
        if (IsWorkerEnough(contest, date, arrHalfHr))
        {
            arrHalfHr++;
            if (arrHalfHr >= contest.Setting.ShiftHalfHrCount)
            {
                arrHalfHr = 0;
                date.AddDays(1);
            }
            ShiftDFS(contest, date, arrHalfHr);
            return;
        }
        //嘗試排班
        foreach (var ss in from staff in contest.GetAvailableStaffs(date)
                 from shiftHalfHr in contest.Setting.ShiftHalfHrType
                 where shiftHalfHr<=contest.Setting.ShiftHalfHrCount-arrHalfHr
                 select new {staff,shiftHalfHr})
        {
            if(!contest.Msm.AssignStaff(date,ss.staff.Id,arrHalfHr,ss.shiftHalfHr,ss.staff.StaffType))
                continue;
            ShiftDFS(contest, date, arrHalfHr);
            contest.Msm.UnassignStaff();
        }
    }

    private void SaveResult(ShiftCreateContest contest)
    {
        contest.IdCount++;
    }

    private bool IsWorkerEnough(ShiftCreateContest contest, DateOnly date, int arrHalfHr)
    {
        var currentWorkers = contest.Msm.GetWorkerCount(date, arrHalfHr);
        var neededWorkers = contest.Setting.EveryHalfHrMinWorkers[arrHalfHr];
        return currentWorkers>=neededWorkers;
    }

    public IMonthlyShiftModel GetShiftModel(DateOnly startDate, List<Staff> staffList, Setting setting)
    {
        throw new NotImplementedException();
    }
}