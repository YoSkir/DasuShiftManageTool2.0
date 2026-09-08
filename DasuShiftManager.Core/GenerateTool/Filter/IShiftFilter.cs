using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.GenerateTool.Filter;

public interface IShiftFilter
{
    List<DailyShift> Filter(List<DailyShift> shifts,ShiftCreateContext context);
}