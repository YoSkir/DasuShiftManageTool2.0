using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.Shift;

public class PrevShiftState:IShiftState
{
    private readonly Dictionary<int,StaffShift> _staffShifts = new();
    public bool AssignStaff(DateOnly date, int staffId, int startArrHalfHr, int workHalfHrs, StaffType staffType)
    {
        throw new NotImplementedException();
    }

    public bool AssignStaffDayOff(DateOnly date, int staffId)
    {
        throw new NotImplementedException();
    }

    public int GetArrHalfHrAssignedStaffCount(DateOnly date, int arrHalfHr)
    {
        throw new NotImplementedException();
    }

    public void UnassignStaff()
    {
        throw new NotImplementedException();
    }

    public bool IsStaffAlreadyAssigned(DateOnly date, int staffId)
    {
        throw new NotImplementedException();
    }

    public int GetChainWorkDays(int staffId)
    {
        throw new NotImplementedException();
    }

    public int GetVacationsOfCurrentWeek(int staffId, DateOnly date)
    {
        throw new NotImplementedException();
    }

    public int GetWorkHalfHrs(int staffId, DateOnly date, int countDays)
    {
        throw new NotImplementedException();
    }

    public ShiftInfo GetShiftCopy(int staffId, DateOnly date)
    {
        throw new NotImplementedException();
    }

    public void AssignShift(Dictionary<int, ShiftInfo> shiftStaffShifts, DateOnly date)
    {
        throw new NotImplementedException();
    }

    public int GetTotalWorkHalfHrs(int staffId)
    {
        throw new NotImplementedException();
    }

    public int GetTotalRestDays(int staffId)
    {
        throw new NotImplementedException();
    }

    public void AssignPto(int staffId)
    {
        throw new NotImplementedException();
    }
}