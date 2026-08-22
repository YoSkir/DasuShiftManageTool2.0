using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.Data;

public class TestDataGetter:IDataGetter
{
    public Setting GetSetting()
    {
        throw new NotImplementedException();
    }

    public List<Staff> GetStaffList()
    {
        throw new NotImplementedException();
    }

    public Dictionary<DateOnly, List<int>> GetVacationStaffList()
    {
        throw new NotImplementedException();
    }
}