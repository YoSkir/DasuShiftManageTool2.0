using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.Models;

namespace DasuShiftManager.Code;

public class ShiftCreateContest(Setting setting,
    Dictionary<DateOnly, List<int>> vacationData, List<Employee> employeeList, MonthlyShiftModel msm)
{
    public List<MonthlyShiftModel> Result { get; init; } = [];
    public Setting Setting { get; init; } = setting;
    public Dictionary<DateOnly, List<int>> VacationData { get; init; } = vacationData;
    public List<Employee> EmployeeList { get; init; } = employeeList;
    public MonthlyShiftModel Msm { get; init; } = msm;
    public int IdCount { get; set; }
}