using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.Models;

namespace DasuShiftManager.Code.GenerateTool;

public class DFSShiftGenerator : IShiftGenerator
{
    public void StartGenerate(ShiftCreateContest contest)
    {
        throw new NotImplementedException();
    }

    public IMonthlyShiftModel GetShiftModel(DateOnly startDate, List<Employee> employeeList, Setting setting)
    {
        throw new NotImplementedException();
    }
}