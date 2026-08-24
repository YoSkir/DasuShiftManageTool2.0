namespace DasuShiftManager.Core.Entities;

public class ShiftInfo
{
    public bool DayOff;
    public int StartArrHalfHr;
    public int WorkHalfHrs;

    public ShiftInfo(int startArrHalfHr, int workHalfHrs)
    {
        DayOff=false;
        StartArrHalfHr = startArrHalfHr;
        WorkHalfHrs = workHalfHrs;
    }

    public ShiftInfo()
    {
        DayOff=true;
    }
}