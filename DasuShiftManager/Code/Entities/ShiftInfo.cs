namespace DasuShiftManager.Code.Entities;

public class ShiftInfo
{
    public bool DayOff;
    public int StartHalfHr;
    public int WorkHalfHrs;

    public ShiftInfo(int startHalfHr, int workHalfHrs)
    {
        DayOff=false;
        StartHalfHr = startHalfHr;
        WorkHalfHrs = workHalfHrs;
    }

    public ShiftInfo()
    {
        DayOff=true;
    }
}