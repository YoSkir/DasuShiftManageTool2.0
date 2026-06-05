using DasuShiftManager.Code.Data;
using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.Models;
using Microsoft.EntityFrameworkCore;

namespace DasuShiftManager.Code;

public class ShiftCreateTool(MyDbContext dbContext,VacationDataGetter vacationDataGetter)
{
    public async Task<List<MonthlyShiftModel>> GenerateThisMonthShift(int year,int month)
    {
        var lastDayOfMonth = DateTime.DaysInMonth(year, month);
        var setting = await dbContext.Setting.FirstOrDefaultAsync();
        if(setting==null) throw new Exception("No settings found");
        var vacationData = vacationDataGetter.GetVacationEmployeeList();
        var employeeList = dbContext.Employee.ToListAsync();
        if (employeeList.Result.Count == 0) throw new Exception("Employee not found");
        
        var result=new List<MonthlyShiftModel>();
        var possibleShiftQueue=new Queue<MonthlyShiftModel>();
        var shiftIdCounter = 0;
        var currentDate = new DateOnly(year, month, setting.ShiftStartDay);
        //Init every possible shift start
        foreach (var employee in employeeList.Result)
        {
            if (vacationData.TryGetValue(currentDate, out var vacationList) &&
                vacationList.Contains(employee.Id))
            {
                var msm = new MonthlyShiftModel(shiftIdCounter++,currentDate,setting.MaxChainWorkDays,setting.ShiftHalfHourCount);
                possibleShiftQueue.Enqueue(msm);
                msm.AddDayOffWorker(currentDate, employee.Id);
                continue;
            }
            if(setting.ShiftHalfHourType.Count==0) setting.ShiftHalfHourType.Add(8);
            var isManager = employee.EmployeeType != EmployeeType.Normal&&employee.EmployeeType!=EmployeeType.Pt;
            foreach (var halfHour in setting.ShiftHalfHourType)
            {
                var msm = new MonthlyShiftModel(shiftIdCounter++,currentDate,setting.MaxChainWorkDays,setting.ShiftHalfHourCount);
                possibleShiftQueue.Enqueue(msm);
                var restIncludedWorkHour = halfHour + GetRestHalfHour(halfHour,setting);
                msm.AddWorker(currentDate, employee.Id, 0, restIncludedWorkHour,isManager);
            }
        }

        //todo 先試試看不篩選最低主管或藥師需求，讓使用者加
        // 或是先篩選，如果沒有任何結果就把這個條件拿掉再跑一次
        while (possibleShiftQueue.Count > 0)
        {
            var msm = possibleShiftQueue.Dequeue();
            currentDate = msm.LastProcessingDate;
        }
        
        //todo 結尾要把每天沒排到的員工加到休假
    }

    private static int GetRestHalfHour(int workHalfHours, Setting setting)
    {
        if(workHalfHours>=setting.SecondBreakActiveWorkHalfHrs) return setting.SecondBreakDurationHalfHrs;
        return workHalfHours >= setting.FirstBreakActiveWorkHalfHrs?setting.FirstBreakDurationHalfHrs:0;
    }
}