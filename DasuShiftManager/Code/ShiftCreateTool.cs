using DasuShiftManager.Code.Data;
using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.GenerateTool;
using DasuShiftManager.Code.Models;
using Microsoft.EntityFrameworkCore;

namespace DasuShiftManager.Code;

public class ShiftCreateTool(DataGetter dataGetter)
{
    public async Task<List<MonthlyShiftModelBFS>> GenerateThisMonthShift(int year,int month,IShiftGenerator generator)
    {
        var setting = dataGetter.GetSetting();
        if(setting==null) throw new Exception("No settings found");
        var vacationData = dataGetter.GetVacationEmployeeList();
        var employeeList = dataGetter.GetEmployeeList();
        if (employeeList.Count == 0) throw new Exception("Employee not found");
        
        var currentDate = new DateOnly(year, month, setting.ShiftStartDay);
        var msm = generator.GetShiftModel(currentDate,employeeList,setting);
        var contest = new ShiftCreateContest(setting, vacationData, employeeList, msm);
        generator.StartGenerate(contest);
    }


    private static int GetRestHalfHour(int workHalfHours, Setting setting)
    {
        if(workHalfHours>=setting.SecondBreakActiveWorkHalfHrs) return setting.SecondBreakDurationHalfHrs;
        return workHalfHours >= setting.FirstBreakActiveWorkHalfHrs?setting.FirstBreakDurationHalfHrs:0;
    }
}