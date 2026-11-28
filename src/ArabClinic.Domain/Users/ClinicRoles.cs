namespace ArabClinic.Domain.Users;

public static class ClinicRoles
{
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";
    public const string Admin = "Admin";
    public const string ClinicalStaff = Doctor + "," + Admin;
}