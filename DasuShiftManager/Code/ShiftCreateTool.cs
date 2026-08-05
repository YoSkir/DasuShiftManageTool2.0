using DasuShiftManager.Code.Data;
using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.GenerateTool;
using DasuShiftManager.Code.Shift;
using Microsoft.EntityFrameworkCore;

namespace DasuShiftManager.Code;

public class ShiftCreateTool(DataGetter dataGetter)
{
    public async Task<ShiftCreateResult> GenerateThisMonthShift(int year,int month,IShiftGenerator generator)
    {
        var setting = dataGetter.GetSetting();
        if(setting==null) throw new Exception("No settings found");
        var vacationData = dataGetter.GetVacationStaffList();
        var staffList = dataGetter.GetStaffList();
        if (staffList.Count == 0) throw new Exception("Staff not found");
        
        //todo 檢查員公數如果不合理(目前想到: 只有一個員工，或員工人數等於少於每日最高可能人數) 則建議使用者招人 並補上最低所需虛擬員工
        
        var currentDate = new DateOnly(year, month, setting.ShiftStartDay);
        var msm = generator.GetShiftModel(currentDate,staffList,setting);
        var contest = new ShiftCreateContest(setting, vacationData, staffList, msm,currentDate);
        generator.StartGenerate(contest);
        
        return contest.GenerateResult();
    }


    private static int GetRestHalfHour(int workHalfHours, Setting setting)
    {
        if(workHalfHours>=setting.SecondBreakActiveWorkHalfHrs) return setting.SecondBreakDurationHalfHrs;
        return workHalfHours >= setting.FirstBreakActiveWorkHalfHrs?setting.FirstBreakDurationHalfHrs:0;
    }
}