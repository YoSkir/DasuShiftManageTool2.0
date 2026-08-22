using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.Data;

public interface IDataGetter
{
    Setting GetSetting();
    List<Staff> GetStaffList();
    Dictionary<DateOnly, List<int>> GetVacationStaffList();
}