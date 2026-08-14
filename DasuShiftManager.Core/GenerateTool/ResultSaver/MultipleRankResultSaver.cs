namespace DasuShiftManager.Core.GenerateTool.ResultSaver;

public class MultipleRankResultSaver:IResultSaver
{
    public void SaveResult(ShiftCreateContext context)
    {
        //檢察總時數
        //檢察總假日
        //分配最佳結果
        context.IdCount++;
    }
}