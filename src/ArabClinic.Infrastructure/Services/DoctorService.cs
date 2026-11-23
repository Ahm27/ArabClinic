using ArabClinic.Application.Doctors;
using ArabClinic.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.Infrastructure.Services;

public sealed class DoctorService(ArabClinicDbContext dbContext) : IDoctorService
{
    public async Task<IReadOnlyList<DoctorDto>> GetAllAsync(int? specializationId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .Include(d => d.Schedules)
            .Include(d => d.DaysOff)
            .Where(d => d.IsApproved);

        if (specializationId.HasValue)
        {
            query = query.Where(d => d.SpecializationId == specializationId.Value);
        }

        var doctors = await query
            .OrderBy(d => d.User != null ? d.User.FullName : d.DoctorId)
            .ToListAsync(cancellationToken);

        return doctors.Select(Map).ToList();
    }

    public async Task<DoctorDto?> GetByIdAsync(string doctorId, CancellationToken cancellationToken = default)
    {
        var doctor = await dbContext.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .Include(d => d.Schedules)
            .Include(d => d.DaysOff)
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId, cancellationToken);

        return doctor == null ? null : Map(doctor);
    }

    private static DoctorDto Map(Domain.Doctors.Doctor d) =>
        new(
            d.DoctorId,
            d.User?.FullName ?? "Doctor",
            d.User?.Email ?? string.Empty,
            d.User?.PhoneNumber,
            d.SpecializationId,
            d.Specialization?.Name ?? "General",
            d.ConsultationFee,
            d.Location,
            d.IsApproved,
            d.Schedules.Select(s => new DoctorScheduleDto(s.ScheduleId, s.DayOfWeek, s.StartTime, s.EndTime)).ToList(),
            d.DaysOff.Select(o => new DoctorDayOffDto(o.DayOffId, o.StartDateTime, o.EndDateTime, o.Reason)).ToList());
}
