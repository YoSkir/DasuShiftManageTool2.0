namespace DasuShiftManager.Code.Entities;

public class Employee
{
    public int Id { get; init; }
    public string Name { get; set; }=string.Empty;
    public EmployeeType EmployeeType { get; set; }
}

public enum EmployeeType
{
    Normal,Manager,Pharmacist,Pt
}