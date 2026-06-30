using DasuShiftManager.Code.Entities;

namespace DasuShiftManager.Code.Models;

public class MonthlyShiftModel
{
    public int Id { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public Dictionary<DateOnly, DailyShiftModel> DailyShiftDic { get; } = new();
    public Dictionary<int, EmployeeMonthlyData> EmployeeMonthlyData { get; } = new();
    public DateOnly LastProcessingDate { get; set; }
    private readonly int _maxChainWorkDays;
    private readonly int _dailyShiftHalfHourCount;
    private readonly int[] _everyHalfHourMinWorkers;
    private readonly int[] _everyHalfHourMinManagersOrPharmacist;
    private readonly List<Employee> _employees;


    public MonthlyShiftModel(int id, DateOnly firstDay,Setting setting,List<Employee> employees)
    {
        Id = id;
        StartDate=firstDay;
        EndDate=StartDate.AddMonths(1).AddDays(-1);
        LastProcessingDate = StartDate;
        _maxChainWorkDays = setting.MaxChainWorkDays;
        _dailyShiftHalfHourCount = setting.ShiftHalfHourCount;
        _everyHalfHourMinWorkers = new int[_dailyShiftHalfHourCount];
        _everyHalfHourMinManagersOrPharmacist = new int[_dailyShiftHalfHourCount];
        int mw = 0, mm = 0;
        for (var i = 0; i < _dailyShiftHalfHourCount; i++)
        {
            mw = setting.EveryHalfHourMinWorkers.GetValueOrDefault(i, mw);
            _everyHalfHourMinWorkers[i] = mw;
            mm = setting.EveryHalfHourMinManagersOrPharmacist.GetValueOrDefault(i, mm);
            _everyHalfHourMinManagersOrPharmacist[i] = mm;
        }
        _employees = employees;
        for (var date = StartDate; date <= EndDate; date = date.AddDays(1))
        {
            DailyShiftDic[date] = new DailyShiftModel(date,_dailyShiftHalfHourCount,_everyHalfHourMinWorkers,_everyHalfHourMinManagersOrPharmacist);
        }
    }

    public MonthlyShiftModel(MonthlyShiftModel copyShift, int id)
    {
        Id = id;
        StartDate = copyShift.StartDate;
        EndDate = copyShift.EndDate;
        LastProcessingDate = copyShift.LastProcessingDate;
        _maxChainWorkDays = copyShift._maxChainWorkDays;
        _dailyShiftHalfHourCount = copyShift._dailyShiftHalfHourCount;
        _everyHalfHourMinWorkers = copyShift._everyHalfHourMinWorkers;
        _everyHalfHourMinManagersOrPharmacist = copyShift._everyHalfHourMinManagersOrPharmacist;
        _employees = copyShift._employees;
        DailyShiftDic = copyShift.DailyShiftDic.ToDictionary(
            pair=>pair.Key,pair=>new DailyShiftModel(pair.Value));
        EmployeeMonthlyData = copyShift.EmployeeMonthlyData.ToDictionary(
            k=>k.Key,k=>new EmployeeMonthlyData(k.Value));
    }

    public bool IsDone()
    {
        return LastProcessingDate > EndDate;
    }

    public bool AddWorker(DateOnly date, int employeeId, int startHalfHour, int workHalfHours,bool isManager)
    {
        var ds = DailyShiftDic[date];
        if(!ds.AddWorker(employeeId, startHalfHour, workHalfHours,isManager)) return false;
        if (ds.IsDone())
        {
            LastProcessingDate = LastProcessingDate.AddDays(1);
            ds.HandleEmptyEmployee(_employees);
        }
        var data=GetEmployeeMonthlyData(employeeId);
        data.TotalWorkHalfHour+=workHalfHours;
        var lastRestDate = data.LastRestDate == DateOnly.MinValue ? StartDate.AddDays(-1) : data.LastRestDate;
        data.LongestChainShift = Math.Max(data.LongestChainShift, date.DayNumber - lastRestDate.DayNumber);
        return data.LongestChainShift<_maxChainWorkDays;
    }

    public bool AddDayOffWorker(DateOnly date, int employeeId)
    {
        if(!DailyShiftDic[date].AddDayOffWorker(employeeId)) return false;
        var data = GetEmployeeMonthlyData(employeeId);
        data.TotalRestDays++;
        data.LastRestDate = date;
        return true;
    }

    public int GetWorkerCount(DateOnly date,int halfHour)
    {
        return DailyShiftDic[date].HalfHourAllTypeWorkerCount[halfHour];
    }
    
    private EmployeeMonthlyData GetEmployeeMonthlyData(int employeeId)
    {
        if (!EmployeeMonthlyData.TryGetValue(employeeId, out var data))
        {
            EmployeeMonthlyData[employeeId] = data = new EmployeeMonthlyData(employeeId);
        }
        return data;
    }
}

public class DailyShiftModel(DateOnly date,int shiftHalfHourCount,int[] everyHalfHourMinWorkers,int[] everyHalfHourMinManagersOrPharmacist)
{
    public DateOnly ShiftDate { get; init; } = date;
    public Dictionary<int, EmployeeShiftModel> EmployeeShiftDic { get; } = new();
    public int[] HalfHourAllTypeWorkerCount { get; } = new int[shiftHalfHourCount];
    public int[] HalfHourManagerCount { get; } = new int[shiftHalfHourCount];
    public HashSet<int> InShiftEmployee { get; } = [];
    public int LastProcessingHalfHour { get; set; }
    private readonly int[] _everyHalfHourMinWorkers=everyHalfHourMinWorkers;
    private readonly int[] _everyHalfHourMinManagersOrPharmacist=everyHalfHourMinManagersOrPharmacist;
    
    public DailyShiftModel(DailyShiftModel copyModel):this(copyModel.ShiftDate,copyModel.HalfHourAllTypeWorkerCount.Length,
        copyModel._everyHalfHourMinWorkers,copyModel._everyHalfHourMinManagersOrPharmacist)
    {
        EmployeeShiftDic = copyModel.EmployeeShiftDic.ToDictionary(p=>p.Key,
            p=>new EmployeeShiftModel(p.Value));
        HalfHourAllTypeWorkerCount=copyModel.HalfHourAllTypeWorkerCount.ToArray();
        HalfHourManagerCount = copyModel.HalfHourManagerCount.ToArray();
        InShiftEmployee=new HashSet<int>(copyModel.InShiftEmployee);
    }

    public bool AddWorker(int workerId, int startHalfHour, int workHalfHours, bool isManager)
    {
        if(!InShiftEmployee.Add(workerId))
        {
            Console.WriteLine($"{ShiftDate.ToShortDateString()} already has employee(id:{workerId}) in shift!");
            return false;
        }
        EmployeeShiftDic[workerId]=new EmployeeShiftModel(workerId, startHalfHour, startHalfHour+workHalfHours);
        for (var i = 0; i < workHalfHours; i++)
        {
            HalfHourAllTypeWorkerCount[startHalfHour+i]++;
            if (isManager)
                HalfHourManagerCount[startHalfHour + i]++;
        }

        return true;
    }

    public bool AddDayOffWorker(int workerId)
    {
        if(!InShiftEmployee.Add(workerId))
        {
            Console.WriteLine($"{ShiftDate.ToShortDateString()} already has employee(id:{workerId}) in shift!");
            return false;
        }
        EmployeeShiftDic[workerId] = new EmployeeShiftModel(workerId);
        return true;
    }

    public void HandleEmptyEmployee(List<Employee> employees)
    {
        foreach (var employee in employees.Where(employee => !InShiftEmployee.Add(employee.Id)))
        {
            EmployeeShiftDic[employee.Id] = new EmployeeShiftModel(employee.Id);
        }
    }

    public bool IsDone()
    {

    }
}

public class EmployeeShiftModel(int id, int startHalfHour = 0, int endHalfHour = 0,bool isBreak = false)
{
    public EmployeeShiftModel(int workerId):this(workerId,-1,-1,true)
    {}

    public EmployeeShiftModel(EmployeeShiftModel copyModel):this(copyModel.EmployeeId,copyModel.StartHalfHour,copyModel.EndHalfHour,copyModel.IsBreak)
    {}

    public int EmployeeId { get; init; } = id;
    public int StartHalfHour { get; set; } = startHalfHour;
    public int EndHalfHour { get; set; } = endHalfHour;
    public bool IsBreak { get; set; } = isBreak;
}

public class EmployeeMonthlyData(int id)
{
    public int EmployeeId { get; init; } = id;
    public int TotalWorkHalfHour { get; set; }
    public int LongestChainShift { get; set; }
    public int TotalRestDays { get; set; }
    public DateOnly LastRestDate { get; set; } = DateOnly.MinValue;
    public EmployeeMonthlyData(EmployeeMonthlyData copyData):this(copyData.EmployeeId)
    {
        TotalWorkHalfHour=copyData.TotalWorkHalfHour;
        LongestChainShift=copyData.LongestChainShift;
        TotalRestDays=copyData.TotalRestDays;
        LastRestDate=copyData.LastRestDate;
    }
}