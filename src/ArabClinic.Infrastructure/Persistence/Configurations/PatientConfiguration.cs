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

        builder.Property(patient => patient.Gender)
            .HasMaxLength(50);

        builder.Property(patient => patient.Address)
            .HasMaxLength(500);

        builder.Property(patient => patient.Allergies)
            .HasMaxLength(1000);

        builder.Property(patient => patient.MedicalHistory)
            .HasMaxLength(2000);

        builder.HasMany(patient => patient.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}