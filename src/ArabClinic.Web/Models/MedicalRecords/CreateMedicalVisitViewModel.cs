using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArabClinic.Web.Models.MedicalRecords;

public sealed class CreateMedicalVisitViewModel
{
    [Required]
    public Guid PatientId { get; set; }

    [Required]
    public string DoctorId { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int? AppointmentId { get; set; }

    [Required]
    public DateTime VisitDate { get; set; } = DateTime.Now;

    [Required, StringLength(4000)]
    public string Summary { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Diagnosis { get; set; } = string.Empty;
    [StringLength(1000)] public string? Symptoms { get; set; }
    [StringLength(2000)] public string? TreatmentNotes { get; set; }
    [StringLength(2000)] public string? FollowUpNotes { get; set; }

    public IReadOnlyList<SelectListItem> PatientOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> DoctorOptions { get; set; } = [];
}