namespace ArabClinic.Domain.Specializations;

public class Specialization
{
    public int SpecializationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Doctors.Doctor> Doctors { get; set; } = [];
}