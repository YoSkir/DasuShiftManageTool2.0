using DasuShiftManager.Core.Data;
using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.Init;

/// <summary>
/// 初始化系統預設排班設定的服務入口。
/// </summary>
public class InitService
{
    /// <summary>
    /// 建立預設排班設定，供後續生成流程使用。
    /// </summary>
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