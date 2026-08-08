namespace DasuShiftManager.Core.Entities;

public class Staff
{
    public int Id { get; init; }
    public string Name { get; set; }=string.Empty;
    public StaffType StaffType { get; set; }
}

public enum StaffType
{
    Normal,Manager,Pharmacist,Pt
}