using DasuShiftManager.Core.Data;
using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.Init;

public class InitService
{
    public void Init()
    {
        var setting = new Setting{
            ShiftStartDay = 1,
            ShiftStartHalfHr = 18,
            ShiftHalfHrCount = 26,
            FirstBreakActiveWorkHalfHrs = 12,
            SecondBreakActiveWorkHalfHrs = 20,
            FirstBreakDurationHalfHrs = 1,
            SecondBreakDurationHalfHrs = 2,
            MaxChainWorkDays = 6,
            MinWeekRestDays = 2,
            MinMonthWorkHrs = 152
        };
    }
}