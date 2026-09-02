using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.Data;

public interface IDataGetter
{
    Setting GetSetting();
    List<Staff> GetStaffList();
    Dictionary<DateOnly, List<int>> GetVacationStaffList();
    Dictionary<int, ShiftInfo?[]> GetFixedShift();
    Dictionary<int,StaffPreferShift> GetPreferShift();
    Dictionary<DateOnly, List<int>> GetPtoStaffList();
}