namespace DasuShiftManager.Core.Entities;

public class ShiftInfo
{
    public bool DayOff;
    public int StartArrHalfHr;
    public int WorkHalfHrs;
    public ShiftType Type;

    public ShiftInfo(int startArrHalfHr, int workHalfHrs)
    {
        DayOff=false;
        StartArrHalfHr = startArrHalfHr;
        WorkHalfHrs = workHalfHrs;
        if (workHalfHrs >= 22) Type = ShiftType.All;
        else if (startArrHalfHr < 13) Type = ShiftType.Early;
        else Type = ShiftType.Late;
    }

    public ShiftInfo()
    {
        DayOff=true;
        Type = ShiftType.Rest;
    }
}

public enum ShiftType
{
    Early,Late,All,Rest
}