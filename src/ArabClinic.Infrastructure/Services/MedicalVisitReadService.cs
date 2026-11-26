using ArabClinic.Application.MedicalRecords;
using ArabClinic.Domain.MedicalRecords;
using ArabClinic.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.Infrastructure.Services;

public sealed class MedicalVisitReadService(ArabClinicDbContext dbContext) : IMedicalVisitReadService
{
    public async Task<IReadOnlyList<MedicalVisitDto>> GetPatientHistoryAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var visits = await QueryVisits()
            .Where(visit => visit.PatientId == patientId)
            .OrderByDescending(visit => visit.VisitDate)
            .ThenByDescending(visit => visit.MedicalVisitId)
            .ToListAsync(cancellationToken);

        return visits.Select(MedicalVisitMapper.Map).ToList();
    }

    public async Task<MedicalVisitDto?> GetByIdAsync(int medicalVisitId, CancellationToken cancellationToken = default)
    {
        var visit = await QueryVisits()
            .FirstOrDefaultAsync(item => item.MedicalVisitId == medicalVisitId, cancellationToken);

        return visit is null ? null : MedicalVisitMapper.Map(visit);
    }

    private IQueryable<MedicalVisit> QueryVisits() => dbContext.MedicalVisits
        .AsNoTracking()
        .Include(visit => visit.Patient)
        .Include(visit => visit.Doctor)
            .ThenInclude(doctor => doctor!.User);
}

internal static class MedicalVisitMapper
{
    public static MedicalVisitDto Map(MedicalVisit visit) => new(
        visit.MedicalVisitId,
        visit.PatientId,
        visit.Patient?.FullName ?? "Patient",
        visit.DoctorId,
        visit.Doctor?.User?.FullName ?? "Doctor",
        visit.AppointmentId,
        visit.VisitDate,
        visit.Summary,
        visit.CreatedAt,
        visit.UpdatedAt);
}
