namespace DasuShiftManager.Code.Entities;

public class DailyShift
{
    public bool DayOff;
    public int StartHalfHr;
    public int WorkHalfHrs;

    public DailyShift(int startHalfHr, int workHalfHrs)
    {
        DayOff=false;
        StartHalfHr = startHalfHr;
        WorkHalfHrs = workHalfHrs;
    }

    public DailyShift()
    {
        DayOff=true;
    }
}