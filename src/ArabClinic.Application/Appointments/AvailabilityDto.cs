namespace ArabClinic.Application.Appointments;

public sealed record AvailableSlotDto(string Value, string Label);

public interface IAvailabilityService
{
    Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(
        string doctorId, DateOnly appointmentDate, CancellationToken cancellationToken = default);
}