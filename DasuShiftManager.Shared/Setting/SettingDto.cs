using System.ComponentModel.DataAnnotations;
using DasuShiftManager.Shared.DataAnnotations;

namespace DasuShiftManager.Server.Entity.Setting;

public class SettingDto
{
    [Required(ErrorMessage = "請輸日班表開始日期")]
    [Range(1,28,ErrorMessage = "請輸入合法日期(1~28)")]
    public int ShiftStartDay { get; set; }
    [Required]
    public TimeSpan? ShiftStartTime { get; set; }
    [Required]
    [NotEqualTo(nameof(ShiftStartTime),ErrorMessage = "開關班時間不可相同")]
    public TimeSpan? ShiftEndTime { get; set; }
    [Required]
    public int FirstBreakActiveWorkHrs { get; set; }
    public int FirstBreakDurationHalfHrs {get; set;}
    public int SecondBreakActiveWorkHrs { get; set; }
    public int SecondBreakDurationHalfHrs { get; set; }
    public List<int> ShiftWorkHrType { get; set; } = [];
    public int[] EveryHalfHrMinWorkers { get; set; } = [];
    public int[] EveryHalfHrMinManagersOrPharmacist { get; set; } = [];
    public int MaxChainWorkDays { get; set; }
    public int MinWeekRestDays { get; set; }
    public int MinMonthWorkHrs { get; set; }
}