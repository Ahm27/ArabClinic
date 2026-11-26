namespace ArabClinic.Application.Accounts;

public sealed record AccountProfile(
    string FullName,
    string Email,
    string? PhoneNumber);