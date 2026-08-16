using DasuShiftManager.Core.Entities;

namespace DasuShiftManager.Core.Data;

/// <summary>
/// 提供排班資料的讀取入口，將外部資料來源與排班邏輯解耦。
/// </summary>
public class DataGetter
{
    /// <summary>
    /// 取得每一天的休假員工清單。
    /// </summary>
    /// <returns>以日期為 key、員工 id 列表為 value 的休假資料。</returns>
    public Dictionary<DateOnly, List<int>> GetVacationStaffList()
    {
        return new();
    }

    /// <summary>
    /// 取得排班設定資料。
    /// </summary>
    /// <returns>目前系統所使用的 <see cref="Setting"/> 實例。</returns>
    /// <exception cref="NotImplementedException">目前資料來源尚未實作時拋出。</exception>
    public Setting GetSetting()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 取得員工清單。
    /// </summary>
    /// <returns>可參與排班的員工集合。</returns>
    /// <exception cref="NotImplementedException">目前資料來源尚未實作時拋出。</exception>
    public List<Staff> GetStaffList()
    {
        throw new NotImplementedException();
    }
}