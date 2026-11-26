namespace ArabClinic.Application.Accounts;

public sealed record RegisterAccountRequest(
    string FullName,
    string Email,
    string? PhoneNumber,
    string Password);