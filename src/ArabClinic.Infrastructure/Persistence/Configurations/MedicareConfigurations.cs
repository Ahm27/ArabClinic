using ArabClinic.Domain.Appointments;
using ArabClinic.Domain.Doctors;
using ArabClinic.Domain.Invoices;
using ArabClinic.Domain.MedicalRecords;
using ArabClinic.Domain.Notifications;
using ArabClinic.Domain.Patients;
using ArabClinic.Domain.Specializations;
using ArabClinic.Domain.Users;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArabClinic.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PhoneNumber).HasMaxLength(50);
        builder.Property(u => u.UserName).HasMaxLength(256);

        builder.HasOne(u => u.Doctor)
            .WithOne(d => d.User)
            .HasForeignKey<Doctor>(d => d.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Patient)
            .WithOne(p => p.User)
            .HasForeignKey<Patient>(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.NotificationPreference)
            .WithOne(p => p.User)
            .HasForeignKey<NotificationPreference>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SpecializationConfiguration : IEntityTypeConfiguration<Specialization>
{
    public void Configure(EntityTypeBuilder<Specialization> builder)
    {
        builder.ToTable("Specializations");
        builder.HasKey(s => s.SpecializationId);
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(s => s.Name).IsUnique();
        builder.Property(s => s.Description).HasMaxLength(500);

        builder.HasMany(s => s.Doctors)
            .WithOne(d => d.Specialization)
            .HasForeignKey(d => d.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");
        builder.HasKey(d => d.DoctorId);
        builder.Property(d => d.ConsultationFee).HasColumnType("decimal(18,2)");
        builder.Property(d => d.Location).HasMaxLength(200).IsRequired();

        builder.HasMany(d => d.Schedules)
            .WithOne(s => s.Doctor)
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.DaysOff)
            .WithOne(o => o.Doctor)
            .HasForeignKey(o => o.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Appointments)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
{
    public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
    {
        builder.ToTable("DoctorSchedules");
        builder.HasKey(s => s.ScheduleId);
    }
}

internal sealed class DoctorDayOffConfiguration : IEntityTypeConfiguration<DoctorDayOff>
{
    public void Configure(EntityTypeBuilder<DoctorDayOff> builder)
    {
        builder.ToTable("DoctorDaysOff");
        builder.HasKey(d => d.DayOffId);
        builder.Property(d => d.Reason).HasMaxLength(500);
    }
}

internal sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(a => a.AppointmentId);
        builder.Property(a => a.Status).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Notes).HasMaxLength(1000);

        builder.HasMany(a => a.StatusHistories)
            .WithOne(h => h.Appointment)
            .HasForeignKey(h => h.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.MedicalRecord)
            .WithOne(m => m.Appointment)
            .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Invoice)
            .WithOne(i => i.Appointment)
            .HasForeignKey<Invoice>(i => i.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class AppointmentStatusHistoryConfiguration : IEntityTypeConfiguration<AppointmentStatusHistory>
{
    public void Configure(EntityTypeBuilder<AppointmentStatusHistory> builder)
    {
        builder.ToTable("AppointmentStatusHistories");
        builder.HasKey(h => h.HistoryId);
        builder.Property(h => h.OldStatus).HasMaxLength(50);
        builder.Property(h => h.NewStatus).HasMaxLength(50).IsRequired();

        builder.HasOne(h => h.ChangedByUser)
            .WithMany()
            .HasForeignKey(h => h.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.ToTable("MedicalRecords");
        builder.HasKey(m => m.MedicalRecordId);
        builder.HasIndex(m => m.AppointmentId).IsUnique();
        builder.Property(m => m.Diagnosis).HasMaxLength(500).IsRequired();
        builder.Property(m => m.Symptoms).HasMaxLength(1000);
        builder.Property(m => m.TreatmentNotes).HasMaxLength(2000);

        builder.HasMany(m => m.Prescriptions)
            .WithOne(p => p.MedicalRecord)
            .HasForeignKey(p => p.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Documents)
            .WithOne(d => d.MedicalRecord)
            .HasForeignKey(d => d.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class MedicalVisitConfiguration : IEntityTypeConfiguration<MedicalVisit>
{
    public void Configure(EntityTypeBuilder<MedicalVisit> builder)
    {
        builder.ToTable("MedicalVisits");
        builder.HasKey(visit => visit.MedicalVisitId);
        builder.Property(visit => visit.DoctorId).HasMaxLength(450).IsRequired();
        builder.Property(visit => visit.Summary).HasMaxLength(4000).IsRequired();
        builder.HasIndex(visit => new { visit.PatientId, visit.VisitDate });

        builder.HasOne(visit => visit.Patient)
            .WithMany()
            .HasForeignKey(visit => visit.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(visit => visit.Doctor)
            .WithMany()
            .HasForeignKey(visit => visit.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(visit => visit.Appointment)
            .WithMany()
            .HasForeignKey(visit => visit.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("Prescriptions");
        builder.HasKey(p => p.PrescriptionId);

        builder.HasMany(p => p.Items)
            .WithOne(i => i.Prescription)
            .HasForeignKey(i => i.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");
        builder.HasKey(i => i.ItemId);
        builder.Property(i => i.MedicationName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Dosage).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Frequency).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Duration).HasMaxLength(100).IsRequired();
    }
}

internal sealed class MedicalDocumentConfiguration : IEntityTypeConfiguration<MedicalDocument>
{
    public void Configure(EntityTypeBuilder<MedicalDocument> builder)
    {
        builder.ToTable("MedicalDocuments");
        builder.HasKey(d => d.DocumentId);
        builder.Property(d => d.FileName).HasMaxLength(255).IsRequired();
        builder.Property(d => d.StorageKey).HasMaxLength(500).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(100).IsRequired();
    }
}

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.InvoiceId);
        builder.HasIndex(i => i.AppointmentId).IsUnique();
        builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.Status).HasMaxLength(50).IsRequired();

        builder.HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.PaymentId);
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.PaymentMethod).HasMaxLength(50).IsRequired();
        builder.Property(p => p.PaymentStatus).HasMaxLength(50).IsRequired();
        builder.Property(p => p.TransactionReference).HasMaxLength(100);
    }
}

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.NotificationId);
        builder.Property(n => n.Message).HasMaxLength(1000).IsRequired();

        builder.HasMany(n => n.DeliveryLogs)
            .WithOne(l => l.Notification)
            .HasForeignKey(l => l.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");
        builder.HasKey(p => p.UserId);
    }
}

internal sealed class NotificationDeliveryLogConfiguration : IEntityTypeConfiguration<NotificationDeliveryLog>
{
    public void Configure(EntityTypeBuilder<NotificationDeliveryLog> builder)
    {
        builder.ToTable("NotificationDeliveryLogs");
        builder.HasKey(l => l.DeliveryLogId);
        builder.Property(l => l.Channel).HasMaxLength(50).IsRequired();
        builder.Property(l => l.DeliveryStatus).HasMaxLength(50).IsRequired();
        builder.Property(l => l.ProviderMessageId).HasMaxLength(200);
        builder.Property(l => l.ErrorMessage).HasMaxLength(1000);
    }
}