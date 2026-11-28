using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArabClinic.Web.Models;

public sealed class DoctorAdminViewModel
{
    [Required] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Phone] public string? PhoneNumber { get; set; }
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int SpecializationId { get; set; }
    [Range(0, 100000)] public decimal ConsultationFee { get; set; }
    [Required] public string Location { get; set; } = string.Empty;
    public int SlotDurationMinutes { get; set; } = 30;
    public IReadOnlyList<SelectListItem> Specializations { get; set; } = [];
}