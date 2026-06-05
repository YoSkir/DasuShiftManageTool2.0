using DasuShiftManager.Code.Data;
using DasuShiftManager.Code.Entities;
using Microsoft.EntityFrameworkCore;

namespace DasuShiftManager.Code.Init;

public class InitService(MyDbContext dbContext)
{
    public async Task Init()
    {
        var setting = await dbContext.Setting.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new Setting{
                ShiftStartDay = 1,
                ShiftStartTime = new TimeOnly(9,0),
                ShiftHalfHourCount = 26,
                FirstBreakActiveWorkHalfHrs = 12,
                SecondBreakActiveWorkHalfHrs = 20,
                FirstBreakDurationHalfHrs = 1,
                SecondBreakDurationHalfHrs = 2,
                MaxChainWorkDays = 5
            };
            await dbContext.AddAsync(setting);
        }
    }
}