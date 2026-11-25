using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArabClinic.Web.Models;

public sealed class AppointmentViewModel
{
    public Guid? PatientId { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a doctor")]
    [Display(Name = "Doctor")]
    public string DoctorId { get; set; } = string.Empty;

    [Display(Name = "Department / Specialty")]
    public string? Department { get; set; }

    [Required(ErrorMessage = "Please select an appointment date")]
    [Display(Name = "Date")]
    [DataType(DataType.Date)]
    public DateOnly? PreferredDate { get; set; }

    [Required(ErrorMessage = "Please select a time slot")]
    [Display(Name = "Time")]
    public TimeSpan? PreferredTime { get; set; }

    [StringLength(500)]
    [Display(Name = "Notes / Symptoms")]
    public string? Notes { get; set; }

    public List<SelectListItem> DoctorOptions { get; set; } = [];
    public List<SelectListItem> DepartmentOptions { get; set; } = [];
}
