using System.Globalization;

using ArabClinic.Application.Appointments;
using ArabClinic.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.Infrastructure.Services;

public sealed class AvailabilityService(ArabClinicDbContext dbContext) : IAvailabilityService
{
    private static readonly HashSet<int> AllowedDurations = [15, 20, 30, 45, 60];

    public async Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(
        string doctorId, DateOnly appointmentDate, CancellationToken cancellationToken = default)
    {
        var doctor = await dbContext.Doctors.AsNoTracking()
            .Include(item => item.Schedules).Include(item => item.DaysOff)
            .FirstOrDefaultAsync(item => item.DoctorId == doctorId && item.IsApproved, cancellationToken);
        if (doctor is null) return [];

        var schedule = doctor.Schedules.FirstOrDefault(item => item.DayOfWeek == appointmentDate.DayOfWeek);
        if (schedule is null) return [];

        var cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        var dayStartLocal = DateTime.SpecifyKind(appointmentDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        var dayEndLocal = dayStartLocal.AddDays(1);
        var dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(dayStartLocal, cairo);
        var dayEndUtc = TimeZoneInfo.ConvertTimeToUtc(dayEndLocal, cairo);
        var booked = await dbContext.Appointments.AsNoTracking()
            .Where(item => item.DoctorId == doctorId && item.Status != "Cancelled"
                && item.StartDateTime >= dayStartUtc && item.StartDateTime < dayEndUtc)
            .Select(item => new { item.StartDateTime, item.EndDateTime }).ToListAsync(cancellationToken);

        var duration = AllowedDurations.Contains(doctor.SlotDurationMinutes)
            ? doctor.SlotDurationMinutes : 30;
        var cursor = dayStartLocal.Add(schedule.StartTime);
        var scheduleEnd = dayStartLocal.Add(schedule.EndTime);
        var result = new List<AvailableSlotDto>();
        while (cursor.AddMinutes(duration) <= scheduleEnd)
        {
            var end = cursor.AddMinutes(duration);
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(cursor, cairo);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(end, cairo);
            var blocked = startUtc <= DateTime.UtcNow
                || doctor.DaysOff.Any(off => startUtc < off.EndDateTime && endUtc > off.StartDateTime)
                || booked.Any(item => startUtc < item.EndDateTime && endUtc > item.StartDateTime);
            if (!blocked)
                result.Add(new AvailableSlotDto(
                    cursor.ToString("HH:mm", CultureInfo.InvariantCulture),
                    cursor.ToString("hh:mm tt", CultureInfo.InvariantCulture)));
            cursor = end;
        }
        return result;
    }
}