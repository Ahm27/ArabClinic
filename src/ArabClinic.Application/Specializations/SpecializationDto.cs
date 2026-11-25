namespace ArabClinic.Application.Specializations;

public sealed record SpecializationDto(
    int SpecializationId,
    string Name,
    string? Description,
    int DoctorCount);

public interface ISpecializationService
{
    Task<IReadOnlyList<SpecializationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SpecializationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
