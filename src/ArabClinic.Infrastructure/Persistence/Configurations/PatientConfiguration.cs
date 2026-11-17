using ArabClinic.Domain.Patients;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArabClinic.Infrastructure.Persistence.Configurations;

internal sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(patient => patient.Id);

        builder.Property(patient => patient.Id)
            .ValueGeneratedNever();

        builder.Property(patient => patient.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(patient => patient.DateOfBirth)
            .HasColumnType("date")
            .IsRequired();
    }
}