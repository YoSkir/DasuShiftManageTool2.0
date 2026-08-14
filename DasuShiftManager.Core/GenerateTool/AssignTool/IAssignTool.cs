namespace DasuShiftManager.Core.GenerateTool.AssignTool;

public interface IAssignTool
{
    void ShiftDfs(ShiftCreateContext context, DateOnly date, int arrHalfHr);
}