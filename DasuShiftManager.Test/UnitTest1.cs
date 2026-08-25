using DasuShiftManager.Core;
using DasuShiftManager.Core.Data;
using DasuShiftManager.Core.GenerateTool;

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
        var generator = new DfsShiftGenerator();
        var res=main.GenerateThisMonthShift(2026,9,generator);
        Assert.That(res.ResultCount, Is.GreaterThan(0));
    }
}