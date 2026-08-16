using System.ComponentModel.DataAnnotations;

namespace DasuShiftManager.Shared.DataAnnotations;
[AttributeUsage(AttributeTargets.Property,AllowMultiple = false)]
public class NotEqualToAttribute : ValidationAttribute
{
    private readonly string _otherPropertyName;

    public NotEqualToAttribute(string otherPropertyName)
    {
        _otherPropertyName = otherPropertyName;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        var otherProperty = ctx.ObjectType.GetProperty(_otherPropertyName);
        if (otherProperty == null)
            return new ValidationResult($"{_otherPropertyName} object type not found!");
        var otherValue = otherProperty.GetValue(ctx.ObjectInstance);
        if (value != null && value.Equals(otherValue))
            return new ValidationResult(ErrorMessage ?? $"This value cant not be equal to {otherProperty.Name}.");
        return ValidationResult.Success;
    }
}