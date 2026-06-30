using DasuShiftManager.Code.Entities;

namespace DasuShiftManager.Code;

public class DataGetter
{
    public Dictionary<DateOnly, List<int>> GetVacationEmployeeList()
    {
        return new();
    }

    public Setting GetSetting()
    {
        throw new NotImplementedException();
    }

    public List<Employee> GetEmployeeList()
    {
        throw new NotImplementedException();
    }
}