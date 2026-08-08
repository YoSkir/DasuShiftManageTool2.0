using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.Shift;

namespace DasuShiftManager.Core.GenerateTool;

public interface IShiftGenerator
{
    void StartGenerate(ShiftCreateContest contest);
    IShiftState GetShiftModel(DateOnly startDate,List<Staff> staffList,Setting setting);
}