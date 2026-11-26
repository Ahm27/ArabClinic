using System.ComponentModel.DataAnnotations;

namespace ArabClinic.Web.Models.Patients;

public sealed class PatientFormViewModel : IValidatableObject
{
    [Required]
    [StringLength(200)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Required]
    [Display(Name = "Gender")]
    public string Gender { get; set; } = "Male";

    [StringLength(500)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [StringLength(1000)]
    [Display(Name = "Known Allergies")]
    public string? Allergies { get; set; }

    [StringLength(2000)]
    [Display(Name = "Medical History")]
    public string? MedicalHistory { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            yield return new ValidationResult(
                "Date of birth cannot be in the future.",
                [nameof(DateOfBirth)]);
        }
    }
}