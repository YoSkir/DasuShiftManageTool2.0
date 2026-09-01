using DasuShiftManager.Core;
using DasuShiftManager.Core.Data;
using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool;
using DasuShiftManager.Core.GenerateTool.AssignTool;
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
        var vacationData = dataGetter.GetVacationStaffList();
        var staffList = dataGetter.GetStaffList();
        if (staffList.Count == 0) throw new Exception("Staff not found");

        //todo 檢查員公數如果不合理(目前想到: 只有一個員工，或員工人數等於少於每日最高可能人數) 則建議使用者招人 並補上最低所需虛擬員工

        var currentDate = new DateOnly(2026, 9, setting.ShiftStartDay);
        var assignTool = new EveryPossibleAssignTool();
        var context = new ShiftCreateContext(setting, vacationData, staffList, currentDate, dataGetter);
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