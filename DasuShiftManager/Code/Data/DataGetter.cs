using DasuShiftManager.Code.Entities;

namespace DasuShiftManager.Code;

public class DataGetter
{
    public Dictionary<DateOnly, List<int>> GetVacationStaffList()
    {
        return new();
    }

    public Setting GetSetting()
    {
        throw new NotImplementedException();
    }

    public List<Staff> GetStaffList()
    {
        throw new NotImplementedException();
    }
}