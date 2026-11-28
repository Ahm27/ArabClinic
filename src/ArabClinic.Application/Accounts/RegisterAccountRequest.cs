namespace ArabClinic.Application.Accounts;

public sealed record RegisterAccountRequest(
    string FullName,
    string Email,
    string? PhoneNumber,
    DateOnly DateOfBirth,
    string Gender,
    string Password);