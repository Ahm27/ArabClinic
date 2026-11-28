namespace ArabClinic.Application.Doctors;

public sealed record DoctorDto(
    string DoctorId,
    string FullName,
    string Email,
    string? PhoneNumber,
    int SpecializationId,
    string SpecializationName,
    decimal ConsultationFee,
    string Location,
    bool IsApproved,
    int SlotDurationMinutes,
    IReadOnlyList<DoctorScheduleDto> Schedules,
    IReadOnlyList<DoctorDayOffDto> DaysOff);

public sealed record DoctorScheduleDto(
    int ScheduleId,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime);

public sealed record DoctorDayOffDto(
    int DayOffId,
    DateTime StartDateTime,
    DateTime EndDateTime,
    string? Reason);

public interface IDoctorService
{
    Task<IReadOnlyList<DoctorDto>> GetAllAsync(int? specializationId = null, CancellationToken cancellationToken = default);
    Task<DoctorDto?> GetByIdAsync(string doctorId, CancellationToken cancellationToken = default);
}