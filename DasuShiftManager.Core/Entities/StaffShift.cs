
using System.Runtime.InteropServices;

namespace DasuShiftManager.Core.Entities;

/// <summary>
/// 保存單一員工在月份中的班表狀態。
/// </summary>
public class StaffShift
{
    private readonly Dictionary<DateOnly, ShiftInfo?> _monthShift = new();
    public int TotalWorkHalfHrs { get; set; }
    public int ChainWorkDays { get; set; }
    public Dictionary<int, int> WeekDayOffCount { get; init; }
    public Dictionary<DateOnly, int> WeekIndex { get; init; }


    public StaffShift(DateOnly startDate,DateOnly endDate)
    {
        WeekDayOffCount =[];
        WeekIndex = [];
        var weekIndex = 1;
        var date = startDate;
        while (date <= endDate)
        {
            WeekIndex[date] = weekIndex;
            if (date.DayOfWeek == DayOfWeek.Sunday)weekIndex++;
            date=date.AddDays(1);
        }
    }
    
    /// <summary>
    /// 判斷指定日期是否已經存在班表紀錄。
    /// </summary>
    /// <param name="date">要檢查的日期。</param>
    /// <returns>若已排班或已排假則為 <see langword="true"/>。</returns>
    public bool IsAlreadyAssigned(DateOnly date)
    {
        return _monthShift.TryGetValue(date, out var shiftInfo) && shiftInfo!=null;
    }

    /// <summary>
    /// 為指定日期記錄休假狀態。
    /// </summary>
    /// <param name="date">休假日期。</param>
    /// <exception cref="InvalidOperationException">若該日期已被安排過則拋出。</exception>
    public void AssignedDayOff(DateOnly date) 
    {
        if(IsAlreadyAssigned(date))
            throw new InvalidOperationException($"While assign day off, date {date.ToShortDateString()} is already assigned");
        _monthShift[date]=new ShiftInfo();
        WeekDayOffCount[WeekIndex[date]]=WeekDayOffCount.GetValueOrDefault(WeekIndex[date],0)+1;
        CollectionsMarshal.GetValueRefOrAddDefault(WeekDayOffCount,WeekIndex[date],out _)++;
        ChainWorkDays = 0;
    }

    /// <summary>
    /// 為指定日期記錄正常工作班別。
    /// </summary>
    /// <param name="date">班表日期。</param>
    /// <param name="startArrHalfHr">開始的半小時索引。</param>
    /// <param name="workHalfHrs">工作時段長度（半小時）。</param>
    /// <exception cref="InvalidOperationException">若該日期已安排過則拋出。</exception>
    public void Assigned(DateOnly date, int startArrHalfHr, int workHalfHrs)
    {
        if(IsAlreadyAssigned(date))
            throw new InvalidOperationException($"While assign work,date {date.ToShortDateString()} is already assigned");
        if (_monthShift.TryGetValue(date.AddDays(-1), out var shiftInfo)&&(shiftInfo==null||shiftInfo.DayOff)) ChainWorkDays = 0;
        ChainWorkDays++;
        TotalWorkHalfHrs+=workHalfHrs;
        _monthShift[date]=new ShiftInfo(startArrHalfHr,workHalfHrs);
    }

    /// <summary>
    /// 撤銷指定日期的班表安排。
    /// </summary>
    /// <param name="date">要撤銷的日期。</param>
    /// <returns>被撤銷的班表資訊。</returns>
    /// <exception cref="InvalidOperationException">若該日期不存在排班紀錄則拋出。</exception>
    public ShiftInfo Unassigned(DateOnly date)
    {
        if (!_monthShift.TryGetValue(date,out var shiftInfo)||shiftInfo==null)
        {
            throw new InvalidOperationException($"Date {date.ToShortDateString()} is not assigned");
        }

        _monthShift[date] = null;

        if (!shiftInfo.DayOff)
        {
            TotalWorkHalfHrs-=shiftInfo.WorkHalfHrs;
            //這裡要注意 因為是遞迴呼叫總是最後一步才能這樣扣連續上班日
            ChainWorkDays = Math.Max(0, ChainWorkDays - 1);
        }
        else
        {
            WeekDayOffCount[WeekIndex[date]]=Math.Max(0,WeekDayOffCount.GetValueOrDefault(WeekIndex[date],0)-1);
        }
        return shiftInfo;
    }

    /// <summary>
    /// 判斷指定日期是否為休假日。
    /// </summary>
    /// <param name="date">日期。</param>
    /// <returns>若為休假或尚未安排排班，則返回 <see langword="true"/>。</returns>
    public bool IsDayOff(DateOnly date)
    {
        if (!_monthShift.TryGetValue(date,out var shiftInfo))
        {
            throw new InvalidOperationException($"Date {date.ToShortDateString()} is not assigned");
        }
        //因為用於遞迴途中往回檢查一周放假天數 所以null也會算放假 故不適合往未來查
        return shiftInfo==null||shiftInfo.DayOff;
    }

    public int GetWorkHalfHrs(DateOnly date)
    {
        return (_monthShift.TryGetValue(date, out var shiftInfo)&&shiftInfo!=null) ? shiftInfo.WorkHalfHrs : 0;
    }

    public ShiftInfo GetShiftCopy(DateOnly date)
    {
        if (!_monthShift.TryGetValue(date, out var shiftInfo)||shiftInfo==null)
            return new ShiftInfo();
        return new ShiftInfo(){DayOff = shiftInfo.DayOff,StartArrHalfHr = shiftInfo.StartArrHalfHr,WorkHalfHrs = shiftInfo.WorkHalfHrs};
    }

    public void Assigned(DateOnly date, ShiftInfo shiftInfo)
    {
        if (shiftInfo.DayOff)
        {
            CollectionsMarshal.GetValueRefOrAddDefault(WeekDayOffCount,WeekIndex[date],out _)++;
            ChainWorkDays = 0;
        }
        else
        {
            ChainWorkDays++;
            TotalWorkHalfHrs+=shiftInfo.WorkHalfHrs;
        }
        _monthShift[date] = shiftInfo;
    }

    public int GetThisWeekDayOff(DateOnly date)
    {
        if (!WeekIndex.TryGetValue(date, out var weekIndex))
        {
            Console.WriteLine($"Week index of date {date.ToShortDateString()} is not assigned");
            return 0;
        }
        return WeekDayOffCount.GetValueOrDefault(weekIndex,0);
    }
}