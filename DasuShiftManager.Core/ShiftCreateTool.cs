using DasuShiftManager.Core.Data;
using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool;
using DasuShiftManager.Core.GenerateTool.AssignTool;
using DasuShiftManager.Core.GenerateTool.ResultSaver;

namespace DasuShiftManager.Core;

public class ShiftCreateTool(DataGetter dataGetter)
{
    public ShiftCreateResult GenerateThisMonthShift(int year,int month,IShiftGenerator generator)
    {
        var setting = dataGetter.GetSetting();
        if(setting==null) throw new Exception("No settings found");
        var vacationData = dataGetter.GetVacationStaffList();
        var staffList = dataGetter.GetStaffList();
        if (staffList.Count == 0) throw new Exception("Staff not found");
        
        //todo 檢查員公數如果不合理(目前想到: 只有一個員工，或員工人數等於少於每日最高可能人數) 則建議使用者招人 並補上最低所需虛擬員工
        
        var currentDate = new DateOnly(year, month, setting.ShiftStartDay);
        var msm = generator.GetShiftModel(currentDate,staffList,setting);
        
        var assignTool = new EveryHalfHrAssignTool();
        var resultSaver = new MultipleRankResultSaver();
        
        var contest = new ShiftCreateContext(setting, vacationData, staffList, msm,currentDate,resultSaver);
        generator.StartGenerate(contest,assignTool);
        
        return contest.GenerateResult();
    }


    private static int GetRestHalfHour(int workHalfHours, Setting setting)
    {
        if(workHalfHours>=setting.SecondBreakActiveWorkHalfHrs) return setting.SecondBreakDurationHalfHrs;
        return workHalfHours >= setting.FirstBreakActiveWorkHalfHrs?setting.FirstBreakDurationHalfHrs:0;
    }
}