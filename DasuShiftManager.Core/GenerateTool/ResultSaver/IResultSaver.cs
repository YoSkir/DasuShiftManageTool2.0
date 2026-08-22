namespace DasuShiftManager.Core.GenerateTool.ResultSaver;

/// <summary>
/// 定義排班結果保存策略，允許不同排序或評分方式處理最終結果。
/// </summary>
public interface IResultSaver
{
    /// <summary>
    /// 將當前已生成的排班結果保存下來。
    /// </summary>
    /// <param name="context">包含當前排班狀態與候選結果的上下文。</param>
    void SaveResult(ShiftCreateContext context);
}