using DasuShiftManager.Core;
using DasuShiftManager.Core.Data;
using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool;
using DasuShiftManager.Core.GenerateTool.AssignTool;
using DasuShiftManager.Core.GenerateTool.Filter;
using DasuShiftManager.Core.GenerateTool.ResultSaver;
using DasuShiftManager.Core.Shift;

namespace DasuShiftManager.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void WeightScoreTest()
    {
        var dataGetter = new TestDataGetter();
        var setting = dataGetter.GetSetting();
        setting.ShiftStartDay = 27;
        setting.MinMonthWorkHrs=144;
        setting.MinMonthRestDays=10;
        var currentDate = new DateOnly(2026, 9, setting.ShiftStartDay);
        var assignTool = new EveryPossibleAssignTool();
        var context = new ShiftCreateContext(setting,currentDate,dataGetter)
        {
            ResultSaver = new DcDfsResultSaver()
        };
        context.EndDate = context.StartDate;
        context.ShiftState = new DfsShiftState(context.StartDate, context.EndDate, context.Setting, context.StaffList);
        assignTool.ShiftDfs(context, context.StartDate, context.NextUndoneArrHalfHr(context.StartDate));
        context.Filter = new WeightedFilter();
        var count = Enum.GetValues<WeightType>().ToDictionary(t => t, t => 0);
        for (var i = 0; i < 1; i++)
        {
            var retries = 0;
            context.EndDate = context.StartDate.AddDays(27);
            context.ShiftState = new DfsShiftState(context.StartDate, context.EndDate, context.Setting, context.StaffList);
            context.ClearShiftTypeCount();
            var date = context.StartDate;
            while (date <= context.EndDate)
            {
                HashSet<int> ptoStaff = [.. context.PtoData.TryGetValue(date, out var ptoList) ? ptoList : []];
                var todayAvailableShift = DcDfsTool.BasicFilter(context,date,ptoStaff);
                //無符合結果時斷開
                if (todayAvailableShift.Count == 0)
                {
                    retries++;
                    if(retries>1000)
                    {
                        foreach (var p in count)
                        {
                            Console.WriteLine($"{p.Key.ToString()}: {p.Value}");
                        }
                        Assert.Fail();
                    }
                    continue;
                }
                var priorityShift = context.Filter.Filter(todayAvailableShift, context);
                var shift = priorityShift.Count == 0
                    ? DcDfsTool.GetRandomDailyShift(todayAvailableShift)
                    : DcDfsTool.GetRandomDailyShift(priorityShift);
                context.AssignShift(shift.StaffShifts, date);
                //特休捕時數
                foreach (var staffId in ptoStaff)
                {
                    context.ShiftState.AssignPto(staffId);
                }
                foreach (var dailyShift in priorityShift)
                {
                    foreach (var p in dailyShift.WeightCount.Count)
                    {
                        count[p.Key] += p.Value;
                    }
                }

                date = date.AddDays(1);
            }
        }

        foreach (var p in count)
        {
            Console.WriteLine($"{p.Key.ToString()}: {p.Value}");
        }
        Assert.Pass();
    }
    
    [Test]
    public void DfsShiftGenerateTest1()
    {
        var dataGetter = new TestDataGetter();
        var main = new ShiftCreateTool(dataGetter);
        var generator = new DcDfsShiftGenerator();
        var res = main.GenerateThisMonthShift(2026, 9, generator);
        Assert.That(res.ResultCount, Is.GreaterThan(0));
        Console.WriteLine($"結果: {res.ResultCount}");
    }

    [Test]
    public void DfsShiftGenerateTest2()
    {
        var dataGetter = new TestDataGetter();
        var setting = dataGetter.GetSetting();
        if (setting == null) throw new Exception("No settings found");
        //todo 檢查員公數如果不合理(目前想到: 只有一個員工，或員工人數等於少於每日最高可能人數) 則建議使用者招人 並補上最低所需虛擬員工

        var currentDate = new DateOnly(2026, 8, setting.ShiftStartDay);
        var assignTool = new EveryPossibleAssignTool();
        var context = new ShiftCreateContext(setting,currentDate, dataGetter);
        //分治法 排出一天所有可能後儲存
        context.ResultSaver = new DcDfsResultSaver();
        context.EndDate = context.StartDate;
        context.ShiftState = new DfsShiftState(context.StartDate, context.EndDate, context.Setting, context.StaffList);
        assignTool.ShiftDfs(context, context.StartDate, context.NextUndoneArrHalfHr(context.StartDate));
        //每日班表組合

        for (var i = 0; i < 100; i++)
        {
            var successCount = 0;
            while (!DcDfsTool.AssignMonthly(context))
            {
                successCount++;
            }
            
            Console.WriteLine($"嘗試次數: {successCount}");
        }
        Assert.That(true);
    }
}