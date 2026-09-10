using DasuShiftManager.Core.Data;
using DasuShiftManager.Core.Entities;
using DasuShiftManager.Core.GenerateTool;
using DasuShiftManager.Core.GenerateTool.AssignTool;
using DasuShiftManager.Core.GenerateTool.ResultSaver;
using DasuShiftManager.Shared;

namespace DasuShiftManager.Core;

/// <summary>
/// 排班生成入口，負責依據設定、員工與休假資料建立整個月份的排班流程。
/// </summary>
public class ShiftCreateTool(IDataGetter dataGetter)
{
    /// <summary>
    /// 產生指定年月的排班結果。
    /// </summary>
    /// <param name="year">排班年份。</param>
    /// <param name="month">排班月份。</param>
    /// <param name="generator">用於啟動排班演算法的生成器。</param>
    /// <returns>本月份的排班結果。</returns>
    /// <exception cref="Exception">設定不存在、員工資料為空等情況時拋出。</exception>
    public ShiftCreateResult GenerateThisMonthShift(int year,int month,IShiftGenerator generator)
    {
        var setting = dataGetter.GetSetting();
        if(setting==null) throw new Exception("No settings found");
        return GenerateThisMonthShift(year, month, generator, new ShiftGenerateDto()
            {
                Day = setting.ShiftStartDay,
                MinWorkHrs = setting.MinMonthWorkHrs,
                MinRestDay = setting.MinMonthRestDays,
                VacationList = dataGetter.GetVacationStaffList(),
                AssignedShiftList = dataGetter.GetAssignedShiftList(),
                PtoStaffList = dataGetter.GetPtoStaffList()
            });
    }
    
    public ShiftCreateResult GenerateThisMonthShift(int year,int month,IShiftGenerator generator,ShiftGenerateDto dto)
    {
        var setting = dataGetter.GetSetting();
        if(setting==null) throw new Exception("No settings found");
        setting.ShiftStartDay = dto.Day;
        setting.MinMonthWorkHrs=dto.MinWorkHrs;
        setting.MinMonthRestDays=dto.MinRestDay;
        var staffList = dataGetter.GetStaffList();
        if (staffList.Count == 0) throw new Exception("Staff not found");
        
        //todo 檢查員公數如果不合理(目前想到: 只有一個員工，或員工人數等於少於每日最高可能人數) 則建議使用者招人 並補上最低所需虛擬員工
        
        var currentDate = new DateOnly(year, month, setting.ShiftStartDay);
        var assignTool = new EveryPossibleAssignTool();
        var context = new ShiftCreateContext(setting,currentDate,dataGetter)
        {
            VacationData = dto.VacationList,
            AssignedShift = dto.AssignedShiftList,
            PtoData = dto.PtoStaffList
        };
        generator.StartGenerate(context,assignTool);
        
        return context.GenerateResult();
    }
}