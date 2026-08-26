using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool.AssignTool;
using DasuShiftManager.Core.Shift;

namespace DasuShiftManager.Core.GenerateTool;

/// <summary>
/// 定義排班生成器的總入口，負責啟動整個演算法流程並建構狀態模型。
/// </summary>
public interface IShiftGenerator
{
    /// <summary>
    /// 啟動排班生成流程。
    /// </summary>
    /// <param name="context">排班執行所需的上下文。</param>
    /// <param name="assignTool">用於遞迴分配班次的演算法工具。</param>
    void StartGenerate(ShiftCreateContext context,IAssignTool assignTool);
    
}