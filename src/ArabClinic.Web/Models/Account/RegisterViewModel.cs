using System.ComponentModel.DataAnnotations;

namespace ArabClinic.Web.Models.Account;

public sealed class RegisterViewModel : IValidatableObject
{
    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Required]
    public string Gender { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool AcceptTerms { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Password.Length < 8)
            yield return new ValidationResult("Password must be at least 8 characters.", [nameof(Password)]);
        if (!Password.Any(char.IsLower))
            yield return new ValidationResult("Password must contain a lowercase letter.", [nameof(Password)]);
        if (!Password.Any(char.IsUpper))
            yield return new ValidationResult("Password must contain an uppercase letter.", [nameof(Password)]);
        if (!Password.Any(char.IsDigit))
            yield return new ValidationResult("Password must contain a number.", [nameof(Password)]);
        if (!Password.Any(character => !char.IsLetterOrDigit(character)))
            yield return new ValidationResult("Password must contain a special character.", [nameof(Password)]);
    }
}