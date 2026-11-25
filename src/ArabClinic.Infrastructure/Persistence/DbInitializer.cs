using ArabClinic.Domain.Appointments;
using ArabClinic.Domain.Doctors;
using ArabClinic.Domain.Invoices;
using ArabClinic.Domain.MedicalRecords;
using ArabClinic.Domain.Notifications;
using ArabClinic.Domain.Patients;
using ArabClinic.Domain.Specializations;
using ArabClinic.Domain.Users;

using Microsoft.EntityFrameworkCore;

namespace ArabClinic.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(ArabClinicDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (await context.Specializations.AnyAsync())
        {
            return;
        }

        // 1. Specializations
        var specializations = new List<Specialization>
        {
            new() { Name = "Cardiology", Description = "Advanced cardiac health, ECG, echocardiography, hypertension and preventative heart care." },
            new() { Name = "Neurology", Description = "Comprehensive brain, spine, migraine management, and neuromuscular disorder care." },
            new() { Name = "Orthopedics", Description = "Bone, joint, sports injury medicine, spine care, and musculoskeletal rehabilitation." },
            new() { Name = "Pediatrics", Description = "Compassionate infant, child, and adolescent healthcare, growth monitoring, and vaccines." },
            new() { Name = "Dermatology", Description = "Skin disease therapies, acne, eczema, laser treatments, and dermatological surgery." },
            new() { Name = "Ophthalmology", Description = "Vision exams, cataract care, retinal health, and advanced ophthalmic treatments." },
            new() { Name = "General Practice", Description = "Primary outpatient care, annual checkups, chronic wellness, and diagnostic triage." }
        };

        await context.Specializations.AddRangeAsync(specializations);
        await context.SaveChangesAsync();

        // 2. Doctor Users & Doctors
        var doctorsData = new[]
        {
            (Id: "doc-sarah-johnson", Name: "Dr. Sarah Johnson", Email: "sarah.johnson@arabclinic.com", Spec: "Cardiology", Fee: 450.00m, Loc: "Building A, Clinic 201"),
            (Id: "doc-michael-chen", Name: "Dr. Michael Chen", Email: "michael.chen@arabclinic.com", Spec: "Neurology", Fee: 500.00m, Loc: "Building B, Clinic 105"),
            (Id: "doc-emily-davis", Name: "Dr. Emily Davis", Email: "emily.davis@arabclinic.com", Spec: "Orthopedics", Fee: 400.00m, Loc: "Building A, Clinic 302"),
            (Id: "doc-robert-smith", Name: "Dr. Robert Smith", Email: "robert.smith@arabclinic.com", Spec: "Pediatrics", Fee: 350.00m, Loc: "Building C, Clinic 101"),
            (Id: "doc-lisa-brown", Name: "Dr. Lisa Brown", Email: "lisa.brown@arabclinic.com", Spec: "Dermatology", Fee: 380.00m, Loc: "Building B, Clinic 204"),
            (Id: "doc-david-wilson", Name: "Dr. David Wilson", Email: "david.wilson@arabclinic.com", Spec: "Ophthalmology", Fee: 420.00m, Loc: "Building A, Clinic 108"),
            (Id: "doc-maria-rodriguez", Name: "Dr. Maria Rodriguez", Email: "maria.rodriguez@arabclinic.com", Spec: "General Practice", Fee: 300.00m, Loc: "Building C, Clinic 102")
        };

        var doctorsList = new List<Doctor>();

        foreach (var item in doctorsData)
        {
            var user = new ApplicationUser
            {
                Id = item.Id,
                FullName = item.Name,
                Email = item.Email,
                UserName = item.Email,
                PhoneNumber = "+20 100 123 45" + item.Id.Length
            };

            await context.Users.AddAsync(user);

            var spec = specializations.First(s => s.Name == item.Spec);
            var doctor = new Doctor
            {
                DoctorId = item.Id,
                SpecializationId = spec.SpecializationId,
                ConsultationFee = item.Fee,
                Location = item.Loc,
                IsApproved = true
            };

            // Standard clinic schedule: Mon - Fri, 9:00 AM - 5:00 PM
            for (var day = DayOfWeek.Monday; day <= DayOfWeek.Friday; day++)
            {
                doctor.Schedules.Add(new DoctorSchedule
                {
                    DoctorId = item.Id,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0)
                });
            }

            // Doctor Day Off
            doctor.DaysOff.Add(new DoctorDayOff
            {
                DoctorId = item.Id,
                StartDateTime = DateTime.UtcNow.Date.AddDays(20).AddHours(8),
                EndDateTime = DateTime.UtcNow.Date.AddDays(22).AddHours(18),
                Reason = "Annual Medical Congress attendance"
            });

            doctorsList.Add(doctor);
        }

        await context.Doctors.AddRangeAsync(doctorsList);
        await context.SaveChangesAsync();

        // 3. Patients
        var patientsData = new[]
        {
            (Id: Guid.Parse("11111111-1111-1111-1111-111111111111"), Name: "Ahmed Ali", Email: "ahmed.ali@example.com", Dob: new DateOnly(1985, 5, 12), Gender: "Male", Addr: "12 El Tahrir St, Downtown Cairo", Allergies: "Penicillin", Hist: "Mild hypertension (diagnosed 2021)"),
            (Id: Guid.Parse("22222222-2222-2222-2222-222222222222"), Name: "Fatima Hassan", Email: "fatima.hassan@example.com", Dob: new DateOnly(1992, 9, 24), Gender: "Female", Addr: "45 Nasr City, Cairo", Allergies: "None", Hist: "Mild seasonal asthma"),
            (Id: Guid.Parse("33333333-3333-3333-3333-333333333333"), Name: "Omar Mahmoud", Email: "omar.mahmoud@example.com", Dob: new DateOnly(1978, 11, 3), Gender: "Male", Addr: "88 Maadi Corniche, Cairo", Allergies: "Sulfa antibiotics", Hist: "Type 2 Diabetes, controlled on Metformin"),
            (Id: Guid.Parse("44444444-4444-4444-4444-444444444444"), Name: "Layla Nour", Email: "layla.nour@example.com", Dob: new DateOnly(2001, 4, 18), Gender: "Female", Addr: "15 Dokki, Giza", Allergies: "Peanuts", Hist: "No chronic conditions"),
            (Id: Guid.Parse("55555555-5555-5555-5555-555555555555"), Name: "Youssef Ibrahim", Email: "youssef.ibrahim@example.com", Dob: new DateOnly(1995, 7, 30), Gender: "Male", Addr: "22 Heliopolis, Cairo", Allergies: "None", Hist: "Right knee ACL reconstruction (2023)")
        };

        var patientsList = new List<Patient>();
        foreach (var p in patientsData)
        {
            var user = new ApplicationUser
            {
                Id = p.Id.ToString(),
                FullName = p.Name,
                Email = p.Email,
                UserName = p.Email,
                PhoneNumber = "+20 111 987 65" + p.Name.Length
            };

            await context.Users.AddAsync(user);

            var patient = new Patient(p.Name, p.Dob, p.Gender, p.Addr, p.Allergies, p.Hist)
            {
                UserId = user.Id
            };
            // Set Id
            typeof(Domain.Common.Entity).GetProperty("Id")?.SetValue(patient, p.Id);

            patientsList.Add(patient);
        }

        await context.Patients.AddRangeAsync(patientsList);
        await context.SaveChangesAsync();

        // 4. Appointments & Medical Records & Invoices
        var baseDate = DateTime.UtcNow.Date;

        // Appointment 1: Completed with Sarah Johnson (Cardiology)
        var app1 = new Appointment
        {
            PatientId = patientsList[0].Id,
            DoctorId = doctorsList[0].DoctorId,
            StartDateTime = baseDate.AddDays(-3).AddHours(10),
            EndDateTime = baseDate.AddDays(-3).AddHours(10).AddMinutes(45),
            Status = "Completed",
            Notes = "Routine cardiac evaluation and blood pressure check.",
            CreatedAt = baseDate.AddDays(-7)
        };

        app1.StatusHistories.Add(new AppointmentStatusHistory
        {
            OldStatus = null,
            NewStatus = "Scheduled",
            ChangedByUserId = doctorsList[0].DoctorId,
            ChangedAt = baseDate.AddDays(-7)
        });
        app1.StatusHistories.Add(new AppointmentStatusHistory
        {
            OldStatus = "Scheduled",
            NewStatus = "Completed",
            ChangedByUserId = doctorsList[0].DoctorId,
            ChangedAt = baseDate.AddDays(-3).AddHours(11)
        });

        var medRec1 = new MedicalRecord
        {
            Diagnosis = "Essential Hypertension - Controlled",
            Symptoms = "Occasional mild morning headache, resting BP 135/85 mmHg.",
            TreatmentNotes = "Continue low-sodium dietary habits. Maintain daily exercise routine. Keep current medication dosage.",
            CreatedAt = baseDate.AddDays(-3).AddHours(11)
        };

        var presc1 = new Prescription
        {
            IssuedAt = baseDate.AddDays(-3).AddHours(11)
        };
        presc1.Items.Add(new PrescriptionItem
        {
            MedicationName = "Amlodipine Besylate",
            Dosage = "5 mg",
            Frequency = "Once daily in the morning",
            Duration = "90 days"
        });
        presc1.Items.Add(new PrescriptionItem
        {
            MedicationName = "CoQ10 Heart Health Supplement",
            Dosage = "100 mg",
            Frequency = "Once daily with food",
            Duration = "30 days"
        });

        medRec1.Prescriptions.Add(presc1);
        medRec1.Documents.Add(new MedicalDocument
        {
            FileName = "ECG_Resting_Report.pdf",
            StorageKey = "docs/ecg_report_ahmed_ali.pdf",
            ContentType = "application/pdf",
            UploadedAt = baseDate.AddDays(-3).AddHours(11)
        });

        app1.MedicalRecord = medRec1;

        var invoice1 = new Invoice
        {
            TotalAmount = doctorsList[0].ConsultationFee,
            Status = "Paid",
            IssuedAt = baseDate.AddDays(-3).AddHours(11)
        };
        invoice1.Payments.Add(new Payment
        {
            Amount = doctorsList[0].ConsultationFee,
            PaymentMethod = "Credit Card",
            PaymentStatus = "Completed",
            PaidAt = baseDate.AddDays(-3).AddHours(11).AddMinutes(10),
            TransactionReference = "TXN-984321-VISA"
        });
        app1.Invoice = invoice1;

        // Appointment 2: Scheduled upcoming appointment
        var app2 = new Appointment
        {
            PatientId = patientsList[1].Id,
            DoctorId = doctorsList[1].DoctorId,
            StartDateTime = baseDate.AddDays(2).AddHours(14),
            EndDateTime = baseDate.AddDays(2).AddHours(14).AddMinutes(30),
            Status = "Scheduled",
            Notes = "Consultation for recurrent migraine symptoms.",
            CreatedAt = baseDate.AddDays(-1)
        };
        app2.StatusHistories.Add(new AppointmentStatusHistory
        {
            OldStatus = null,
            NewStatus = "Scheduled",
            ChangedByUserId = doctorsList[1].DoctorId,
            ChangedAt = baseDate.AddDays(-1)
        });

        var invoice2 = new Invoice
        {
            TotalAmount = doctorsList[1].ConsultationFee,
            Status = "Unpaid",
            IssuedAt = baseDate.AddDays(-1)
        };
        app2.Invoice = invoice2;

        // Appointment 3: Scheduled with Dr. Emily Davis (Orthopedics)
        var app3 = new Appointment
        {
            PatientId = patientsList[4].Id,
            DoctorId = doctorsList[2].DoctorId,
            StartDateTime = baseDate.AddDays(3).AddHours(11),
            EndDateTime = baseDate.AddDays(3).AddHours(11).AddMinutes(45),
            Status = "Scheduled",
            Notes = "Follow-up check on right knee post-operative stability.",
            CreatedAt = baseDate
        };
        app3.StatusHistories.Add(new AppointmentStatusHistory
        {
            OldStatus = null,
            NewStatus = "Scheduled",
            ChangedByUserId = doctorsList[2].DoctorId,
            ChangedAt = baseDate
        });

        var invoice3 = new Invoice
        {
            TotalAmount = doctorsList[2].ConsultationFee,
            Status = "Unpaid",
            IssuedAt = baseDate
        };
        app3.Invoice = invoice3;

        // Appointment 4: Cancelled appointment
        var app4 = new Appointment
        {
            PatientId = patientsList[2].Id,
            DoctorId = doctorsList[6].DoctorId,
            StartDateTime = baseDate.AddDays(-1).AddHours(15),
            EndDateTime = baseDate.AddDays(-1).AddHours(15).AddMinutes(30),
            Status = "Cancelled",
            Notes = "Patient requested cancellation due to travel schedule.",
            CreatedAt = baseDate.AddDays(-5)
        };
        app4.StatusHistories.Add(new AppointmentStatusHistory
        {
            OldStatus = null,
            NewStatus = "Scheduled",
            ChangedByUserId = doctorsList[6].DoctorId,
            ChangedAt = baseDate.AddDays(-5)
        });
        app4.StatusHistories.Add(new AppointmentStatusHistory
        {
            OldStatus = "Scheduled",
            NewStatus = "Cancelled",
            ChangedByUserId = patientsList[2].Id.ToString(),
            ChangedAt = baseDate.AddDays(-2)
        });

        await context.Appointments.AddRangeAsync(app1, app2, app3, app4);
        await context.SaveChangesAsync();

        // 5. Notifications & Preferences
        var notif1 = new Notification
        {
            UserId = patientsList[0].Id.ToString(),
            Message = "Your consultation with Dr. Sarah Johnson has been completed. Your medical record and prescription are ready.",
            IsRead = true,
            CreatedAt = baseDate.AddDays(-3).AddHours(11)
        };
        notif1.DeliveryLogs.Add(new NotificationDeliveryLog
        {
            Channel = "Email",
            DeliveryStatus = "Sent",
            ProviderMessageId = "MSG-EMAIL-823910",
            SentAt = baseDate.AddDays(-3).AddHours(11).AddMinutes(1)
        });

        var notif2 = new Notification
        {
            UserId = patientsList[1].Id.ToString(),
            Message = "Reminder: You have an upcoming appointment with Dr. Michael Chen in Neurology on " + baseDate.AddDays(2).ToShortDateString() + " at 2:00 PM.",
            IsRead = false,
            CreatedAt = baseDate.AddDays(-1)
        };
        notif2.DeliveryLogs.Add(new NotificationDeliveryLog
        {
            Channel = "SMS",
            DeliveryStatus = "Sent",
            ProviderMessageId = "MSG-SMS-449120",
            SentAt = baseDate.AddDays(-1).AddMinutes(5)
        });

        await context.Notifications.AddRangeAsync(notif1, notif2);

        foreach (var p in patientsList)
        {
            await context.NotificationPreferences.AddAsync(new NotificationPreference
            {
                UserId = p.Id.ToString(),
                EmailEnabled = true,
                SmsEnabled = true
            });
        }

        await context.SaveChangesAsync();
    }
}
