namespace ArabClinic.Application.Doctors;

public sealed record CreateDoctorRequest(
    string FullName, string Email, string? PhoneNumber, string Password,
    int SpecializationId, decimal ConsultationFee, string Location,
    int SlotDurationMinutes);

public interface IAdminDoctorService
{
    Task<IReadOnlyList<DoctorDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<(bool Success, IReadOnlyList<string> Errors)> CreateAsync(
        CreateDoctorRequest request, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(string doctorId, bool active, CancellationToken cancellationToken = default);
}