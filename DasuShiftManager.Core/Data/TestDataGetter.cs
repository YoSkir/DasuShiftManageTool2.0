using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.Data;

public class TestDataGetter:IDataGetter
{
    public Setting GetSetting()
    {
        var res= new Setting{
            ShiftStartDay = 2,
            ShiftStartHalfHr = 18,
            ShiftHalfHrCount = 26,
            FirstBreakActiveWorkHalfHrs = 12,
            SecondBreakActiveWorkHalfHrs = 20,
            FirstBreakDurationHalfHrs = 1,
            SecondBreakDurationHalfHrs = 2,
            MaxChainWorkDays = 6,
            MinWeekRestDays = 2,
            MinMonthWorkHrs = 152,
            MinMonthRestDays = 9,
            ShiftHalfHrType = [13,17,22,26],
            EveryHalfHrMinWorkers = 
            [2,2,
                2,2,
                3,3,
                3,3,
                3,3,
                3,3,
                3,3,
                3,3,
                3,3,
                3,3,
                3,3,
                3,3,
                3,3],
        };
        return res;
    }

    public List<Staff> GetStaffList()
    {
        var res=new List<Staff>();
        res.Add(new Staff()
        {
            Id = 1,
            Name="吳玟頤",
            StaffType = StaffType.Manager
        });
        res.Add(new Staff()
        {
            Id = 2,
            Name="沈煌偉",
            StaffType = StaffType.Pharmacist
        });
        res.Add(new Staff()
        {
            Id = 3,
            Name="周怡伶",
            StaffType = StaffType.Normal
        });
        res.Add(new Staff()
        {
            Id = 4,
            Name="郭婷芳",
            StaffType = StaffType.Normal
        });
        res.Add(new Staff()
        {
            Id = 5,
            Name="陳姿涵",
            StaffType = StaffType.Normal
        });
        
        return res;
    }

    public Dictionary<DateOnly, List<int>> GetVacationStaffList()
    {
        return [];
    }

    public Dictionary<int, ShiftInfo?[]> GetFixedShift()
    {
        var res=new Dictionary<int, ShiftInfo?[]>();
        var dayOff = new ShiftInfo();
        ShiftInfo? blank = null;
        res[2] = [dayOff,blank,blank,dayOff,blank,blank,dayOff];
        return res;
    }

    public Dictionary<int, StaffPreferShift> GetPreferShift()
    {
        var res = new Dictionary<int, StaffPreferShift>();
        res[2] = new StaffPreferShift(){StartArrHalfHr =0};
        return res;
    }

    public Dictionary<DateOnly, List<int>> GetPtoStaffList()
    {
        throw new NotImplementedException();
    }
}