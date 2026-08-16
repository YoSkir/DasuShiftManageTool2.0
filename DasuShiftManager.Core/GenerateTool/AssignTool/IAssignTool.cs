namespace DasuShiftManager.Core.GenerateTool.AssignTool;

/// <summary>
/// 定義排班遞迴演算法的抽象行為。
/// </summary>
public interface IAssignTool
{
    /// <summary>
    /// 使用指定的排班上下文，依序嘗試在某日期與半小時點建立班表。
    /// </summary>
    /// <param name="context">目前的排班上下文。</param>
    /// <param name="date">目前要處理的日期。</param>
    /// <param name="arrHalfHr">目前要處理的半小時索引。</param>
    void ShiftDfs(ShiftCreateContext context, DateOnly date, int arrHalfHr);
}