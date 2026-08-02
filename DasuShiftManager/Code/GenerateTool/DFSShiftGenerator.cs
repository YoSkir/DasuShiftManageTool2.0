using DasuShiftManager.Code.Entities;
using DasuShiftManager.Code.Models;

namespace DasuShiftManager.Code.GenerateTool;

public class DFSShiftGenerator : IShiftGenerator
{
    public void StartGenerate(ShiftCreateContest contest)
    {
        throw new NotImplementedException();
        //todo 遞迴時 每圈都讓每個員工每個班別嘗試排入
        //每圈都依當日人數自動推進時程與日期 直到當月全排完，填入無排入員工之放假後 把員工班表寫入資料庫
        //每圈開始前依當日先排除請假員工、固定班別員工、當日已排員工
        //請假與固定班別員工不計入歷史但計入人數與個別班表 這樣回溯只會嘗試可排員工
        //這樣能回溯也可以排到每種可能
        //todo 思考回溯時是否也包含推進日期與時程 如果沒有的話 可能每輪都需要從第一天開始掃描
    }

    public IMonthlyShiftModel GetShiftModel(DateOnly startDate, List<Employee> employeeList, Setting setting)
    {
        throw new NotImplementedException();
    }
}