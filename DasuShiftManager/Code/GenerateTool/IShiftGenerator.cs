using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.Models;

namespace DasuShiftManager.Code.GenerateTool;

public interface IShiftGenerator
{
    void StartGenerate(ShiftCreateContest contest);
    IMonthlyShiftModel GetShiftModel(DateOnly startDate,List<Staff> staffList,Setting setting);
}