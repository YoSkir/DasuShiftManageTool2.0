namespace DasuShiftManager.Core.GenerateTool.ResultSaver;

public class DcDfsResultSaver:IResultSaver
{
    public void SaveResult(ShiftCreateContext context)
    {
        context.IdCount++;
    }
}