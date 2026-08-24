namespace DasuShiftManager.Core.GenerateTool.ResultSaver;

/// <summary>
/// 保存多候選結果並依照評分邏輯統計最終結果數量。
/// </summary>
public class MultipleRankResultSaver:IResultSaver
{
    /// <summary>
    /// 將目前完成的排班方案保存為候選結果。
    /// </summary>
    /// <param name="context">包含當前排班狀態的上下文。</param>
    public void SaveResult(ShiftCreateContext context)
    {
        //檢察總時數
        //檢察總假日
        //分配最佳結果
        context.IdCount++;
        Console.WriteLine($"班表完成 {context.IdCount}");
    }
}