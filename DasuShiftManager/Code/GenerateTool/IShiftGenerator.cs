using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.Shift;

namespace DasuShiftManager.Code.GenerateTool;

public interface IShiftGenerator
{
    void StartGenerate(ShiftCreateContest contest);
    IShiftState GetShiftModel(DateOnly startDate,List<Staff> staffList,Setting setting);
}