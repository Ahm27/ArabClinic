using System.ComponentModel.DataAnnotations;

namespace ArabClinic.Api.Models;

public sealed class AppointmentViewModel
{
    [Required, StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, Phone]
    public string Phone { get; init; } = string.Empty;

    [Required]
    [Display(Name = "Preferred date")]
    [DataType(DataType.Date)]
    public DateOnly? PreferredDate { get; init; }

    [Required]
    public string Department { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; init; }
}