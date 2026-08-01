using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.Models;

namespace DasuShiftManager.Code.GenerateTool;

public class BFSShiftGenerator : IShiftGenerator
{
    public void StartGenerate(ShiftCreateContest contest)
    {
        //Init every possible shift start
        // foreach (var employee in employeeList.Result)
        // {
        //     if (vacationData.TryGetValue(currentDate, out var vacationList) &&
        //         vacationList.Contains(employee.Id))
        //     {
        //         var msm = new MonthlyShiftModelWFS(shiftIdCounter++,currentDate,setting.MaxChainWorkDays,setting.ShiftHalfHourCount);
        //         possibleShiftQueue.Enqueue(msm);
        //         msm.AddDayOffWorker(currentDate, employee.Id);
        //         continue;
        //     }
        //     if(setting.ShiftHalfHourType.Count==0) setting.ShiftHalfHourType.Add(8);
        //     var isManager = employee.EmployeeType != EmployeeType.Normal&&employee.EmployeeType!=EmployeeType.Pt;
        //     foreach (var halfHour in setting.ShiftHalfHourType)
        //     {
        //         var msm = new MonthlyShiftModelWFS(shiftIdCounter++,currentDate,setting.MaxChainWorkDays,setting.ShiftHalfHourCount);
        //         possibleShiftQueue.Enqueue(msm);
        //         var restIncludedWorkHour = halfHour + GetRestHalfHour(halfHour,setting);
        //         msm.AddWorker(currentDate, employee.Id, 0, restIncludedWorkHour,isManager);
        //     }
        // }

        //todo 先試試看不篩選最低主管或藥師需求，讓使用者加
        // 或是先篩選，如果沒有任何結果就把這個條件拿掉再跑一次
        
        // while (possibleShiftQueue.Count > 0)
        // {
        //     var msm = possibleShiftQueue.Dequeue();
        //     currentDate = msm.LastProcessingDate;
        // }
        
        //todo 結尾要把每天沒排到的員工加到休假
    }

    public IMonthlyShiftModel GetShiftModel(DateOnly startDate, List<Employee> employeeList, Setting setting)
    {
        return new MonthlyShiftModelBFS(startDate, setting, employeeList);
    }
}