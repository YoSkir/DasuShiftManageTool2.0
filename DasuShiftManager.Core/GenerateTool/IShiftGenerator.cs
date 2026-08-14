using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool.AssignTool;
using DasuShiftManager.Core.Shift;

namespace DasuShiftManager.Core.GenerateTool;

public interface IShiftGenerator
{
    void StartGenerate(ShiftCreateContext context,IAssignTool assignTool);
    IShiftState GetShiftModel(DateOnly startDate,List<Staff> staffList,Setting setting);
}