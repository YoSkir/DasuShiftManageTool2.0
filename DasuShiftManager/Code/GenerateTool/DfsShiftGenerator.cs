using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.Shift;

namespace DasuShiftManager.Code.GenerateTool;

public class DfsShiftGenerator : IShiftGenerator
{
    public void StartGenerate(ShiftCreateContest contest)
    {
        //劃假排入
        AssignDayOff(contest);
        //固定班別排入
        AssignFixedShiftStaff(contest);
        //遞迴排班
        ShiftDfs(contest,contest.StartDate,0);
    }

    private void AssignDayOff(ShiftCreateContest contest)
    {
        foreach (var dayOffData in from pair in contest.VacationData
                 let date=pair.Key
                 from id in pair.Value
                 select new {date,id})
        {
            contest.ShiftState.AssignStaffDayOff(dayOffData.date,dayOffData.id);
        }
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
                if(!contest.ShiftState.AssignStaff(date, fixedPair.Key, shift.StartHalfHr, shift.WorkHalfHrs, StaffType.Normal))
                    throw new InvalidOperationException($"Fixed shift assignment failed, staff id: {fixedPair.Key}");
            }
            date.AddDays(1);
        }
    }

    private void ShiftDfs(ShiftCreateContest contest, DateOnly date, int arrHalfHr)
    {
        //存結果條件
        if (date >= contest.StartDate.AddMonths(1))
        {
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
                //這裡不順便補上沒排班人員的假日 是因為會擾亂遞迴歷史紀錄
            }
            ShiftDfs(contest, date, arrHalfHr);
            return;
        }
        //嘗試排班
        foreach (var ss in from staff in contest.GetAvailableStaffs(date)
                 from shiftHalfHr in contest.Setting.ShiftHalfHrType
                 where shiftHalfHr<=contest.Setting.ShiftHalfHrCount-arrHalfHr
                 select new {staff,shiftHalfHr})
        {
            if(!contest.ShiftState.AssignStaff(date,ss.staff.Id,arrHalfHr,ss.shiftHalfHr,ss.staff.StaffType))
                continue;
            ShiftDfs(contest, date, arrHalfHr);
            contest.ShiftState.UnassignStaff();
        }
    }

    private void SaveResult(ShiftCreateContest contest)
    {
        //檢察總時數
        //檢察總假日
        //分配最佳結果
        contest.IdCount++;
    }

    private bool IsWorkerEnough(ShiftCreateContest contest, DateOnly date, int arrHalfHr)
    {
        var currentWorkers = contest.ShiftState.GetWorkerCount(date, arrHalfHr);
        var neededWorkers = contest.Setting.EveryHalfHrMinWorkers[arrHalfHr];
        return currentWorkers>=neededWorkers;
    }

    public IShiftState GetShiftModel(DateOnly startDate, List<Staff> staffList, Setting setting)
    {
        return new ShiftStateDfs(startDate,setting,staffList);
    }
}