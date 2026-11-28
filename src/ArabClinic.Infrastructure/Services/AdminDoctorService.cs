using ArabClinic.Application.Doctors;
using ArabClinic.Domain.Doctors;
using ArabClinic.Domain.Users;
using ArabClinic.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArabClinic.Infrastructure.Services;

public sealed class AdminDoctorService(UserManager<ApplicationUser> userManager, ArabClinicDbContext dbContext)
    : IAdminDoctorService
{
    private static readonly HashSet<int> AllowedDurations = [15, 20, 30, 45, 60];

    public async Task<IReadOnlyList<DoctorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await dbContext.Doctors.AsNoTracking().Include(item => item.User)
            .Include(item => item.Specialization).Include(item => item.Schedules).Include(item => item.DaysOff)
            .OrderBy(item => item.User!.FullName).ToListAsync(cancellationToken);
        return items.Select(item => new DoctorDto(item.DoctorId, item.User?.FullName ?? "Doctor", item.User?.Email ?? "",
            item.User?.PhoneNumber, item.SpecializationId, item.Specialization?.Name ?? "General", item.ConsultationFee,
            item.Location, item.IsApproved, item.SlotDurationMinutes,
            item.Schedules.Select(s => new DoctorScheduleDto(s.ScheduleId, s.DayOfWeek, s.StartTime, s.EndTime)).ToList(),
            item.DaysOff.Select(d => new DoctorDayOffDto(d.DayOffId, d.StartDateTime, d.EndDateTime, d.Reason)).ToList())).ToList();
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> CreateAsync(
        CreateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        if (!AllowedDurations.Contains(request.SlotDurationMinutes))
            return (false, ["Slot duration must be 15, 20, 30, 45 or 60 minutes."]);
        if (!await dbContext.Specializations.AnyAsync(item => item.SpecializationId == request.SpecializationId, cancellationToken))
            return (false, ["Specialization was not found."]);

        var email = request.Email.Trim();
        var user = new ApplicationUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            UserName = email,
            PhoneNumber = request.PhoneNumber?.Trim(),
            EmailConfirmed = true
        };
        var create = await userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded) return (false, create.Errors.Select(item => item.Description).ToArray());
        var role = await userManager.AddToRoleAsync(user, ClinicRoles.Doctor);
        if (!role.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return (false, role.Errors.Select(item => item.Description).ToArray());
        }

        var doctor = new Doctor
        {
            DoctorId = user.Id,
            SpecializationId = request.SpecializationId,
            ConsultationFee = request.ConsultationFee,
            Location = request.Location.Trim(),
            SlotDurationMinutes = request.SlotDurationMinutes,
            IsApproved = true
        };
        for (var day = DayOfWeek.Sunday; day <= DayOfWeek.Thursday; day++)
            doctor.Schedules.Add(new DoctorSchedule { DoctorId = user.Id, DayOfWeek = day, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17) });
        await dbContext.Doctors.AddAsync(doctor, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, []);
    }

    public async Task<bool> SetActiveAsync(string doctorId, bool active, CancellationToken cancellationToken = default)
    {
        var doctor = await dbContext.Doctors.FirstOrDefaultAsync(item => item.DoctorId == doctorId, cancellationToken);
        if (doctor is null) return false;
        doctor.IsApproved = active;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}