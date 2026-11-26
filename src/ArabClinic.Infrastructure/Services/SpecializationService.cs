using ArabClinic.Application.Specializations;
using ArabClinic.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.Infrastructure.Services;

public sealed class SpecializationService(ArabClinicDbContext dbContext) : ISpecializationService
{
    public async Task<IReadOnlyList<SpecializationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var specializations = await dbContext.Specializations
            .AsNoTracking()
            .Include(s => s.Doctors)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return specializations
            .Select(s => new SpecializationDto(s.SpecializationId, s.Name, s.Description, s.Doctors.Count(d => d.IsApproved)))
            .ToList();
    }

    public async Task<SpecializationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var spec = await dbContext.Specializations
            .AsNoTracking()
            .Include(s => s.Doctors)
            .FirstOrDefaultAsync(s => s.SpecializationId == id, cancellationToken);

        return spec == null
            ? null
            : new SpecializationDto(spec.SpecializationId, spec.Name, spec.Description, spec.Doctors.Count(d => d.IsApproved));
    }
}