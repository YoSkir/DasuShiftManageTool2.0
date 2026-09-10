using DasuShiftManager.Core.Entities;
using DasuShiftManager.Shared;

namespace DasuShiftManager.Core.Data;

public interface IDataGetter
{
    Setting GetSetting();
    List<Staff> GetStaffList();
    Dictionary<DateOnly, List<int>> GetVacationStaffList();
    Dictionary<int, Dictionary<DateOnly, ShiftInfo>> GetAssignedShiftList();
    Dictionary<int, ShiftInfo?[]> GetFixedShift();
    Dictionary<int,StaffPreferShift> GetPreferShift();
    Dictionary<DateOnly, List<int>> GetPtoStaffList();
    Dictionary<DayOfWeek,HalfHrWorkers> GetHalfHrWorkers();
}