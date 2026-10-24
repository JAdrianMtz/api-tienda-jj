using System.ComponentModel.DataAnnotations;

namespace Api.Validations;

public class StrongPasswordAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string password || string.IsNullOrWhiteSpace(password))
            return ValidationResult.Success;

        var errors = new List<string>();

        if (!password.Any(char.IsUpper))
            errors.Add("una mayúscula");

        if (!password.Any(char.IsLower))
            errors.Add("una minúscula");

        if (!password.Any(char.IsDigit))
            errors.Add("un número");

        if (!password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
            errors.Add("un carácter especial");

        if (password.Any(char.IsWhiteSpace))
            errors.Add("sin espacios");

        if (errors.Count == 0)
            return ValidationResult.Success;

        string propertyName = $" {validationContext.MemberName}" ?? "";
        return new ValidationResult($"El campo{propertyName} no cumple con los siguientes requisitos: {string.Join(", ", errors)}.");
    }
}
